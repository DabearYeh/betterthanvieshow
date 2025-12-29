using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using betterthanvieshow.Hubs;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 訂單服務實作
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IDailyScheduleRepository _dailyScheduleRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IHubContext<ShowtimeHub> _hubContext;

    public OrderService(
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        IShowtimeRepository showtimeRepository,
        IDailyScheduleRepository dailyScheduleRepository,
        ISeatRepository seatRepository,
        IHubContext<ShowtimeHub> hubContext)
    {
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _showtimeRepository = showtimeRepository;
        _dailyScheduleRepository = dailyScheduleRepository;
        _seatRepository = seatRepository;
        _hubContext = hubContext;
    }

    /// <inheritdoc/>
    public async Task<CreateOrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequestDto request)
    {
        // 1. 驗證場次存在並加載關聯資料
        var showtime = await _showtimeRepository.GetByIdWithDetailsAsync(request.ShowTimeId);
        if (showtime == null)
        {
            throw new InvalidOperationException($"場次 ID {request.ShowTimeId} 不存在");
        }

        // 2. 驗證時刻表狀態為 OnSale
        var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(showtime.ShowDate);
        if (dailySchedule == null || dailySchedule.Status != "OnSale")
        {
            throw new InvalidOperationException($"場次日期 {showtime.ShowDate} 的時刻表尚未開放販售");
        }

        // 3. 驗證座位數量（1-6張）
        if (request.SeatIds.Count < 1 || request.SeatIds.Count > 6)
        {
            throw new InvalidOperationException("每張訂單最少需選擇 1 個座位，最多只能選擇 6 個座位");
        }

        // 4. 驗證座位存在
        var seats = await _seatRepository.GetByIdsAsync(request.SeatIds);
        if (seats.Count != request.SeatIds.Count)
        {
            var foundIds = seats.Select(s => s.Id).ToList();
            var missingIds = request.SeatIds.Except(foundIds).ToList();
            throw new InvalidOperationException($"座位 ID {string.Join(", ", missingIds)} 不存在");
        }

        // 5. 驗證座位未被訂購
        foreach (var seatId in request.SeatIds)
        {
            var isOccupied = await _ticketRepository.IsSeatOccupiedAsync(request.ShowTimeId, seatId);
            if (isOccupied)
            {
                var seat = seats.First(s => s.Id == seatId);
                throw new InvalidOperationException($"座位 {seat.RowName}{seat.ColumnNumber} 已被訂購");
            }
        }

        // 6. 生成唯一訂單編號
        var orderNumber = await GenerateUniqueOrderNumberAsync();

        // 7. 計算票價（根據影廳類型）
        var ticketPrice = CalculateTicketPrice(showtime.Theater.Type);
        var totalPrice = ticketPrice * request.SeatIds.Count;

        // 8. 創建訂單記錄
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            ShowTimeId = request.ShowTimeId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Status = "Pending",
            TotalPrice = totalPrice,
            TicketCount = request.SeatIds.Count
        };

        order = await _orderRepository.CreateAsync(order);

        // 9. 批次創建票券記錄
        var tickets = new List<Ticket>();
        foreach (var seatId in request.SeatIds)
        {
            var ticketNumber = await GenerateUniqueTicketNumberAsync();
            var ticket = new Ticket
            {
                TicketNumber = ticketNumber,
                OrderId = order.Id,
                ShowTimeId = request.ShowTimeId,
                SeatId = seatId,
                Status = "Pending",
                Price = ticketPrice
            };
            tickets.Add(ticket);
        }

        tickets = await _ticketRepository.CreateBatchAsync(tickets);

        // 10. 組裝回應 DTO
        var response = new CreateOrderResponseDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TotalPrice = order.TotalPrice,
            ExpiresAt = order.ExpiresAt,
            TicketCount = order.TicketCount,
            Seats = tickets.Select(t =>
            {
                var seat = seats.First(s => s.Id == t.SeatId);
                return new SeatInfoDto
                {
                    SeatId = seat.Id,
                    RowName = seat.RowName,
                    ColumnNumber = seat.ColumnNumber,
                    TicketNumber = t.TicketNumber
                };
            }).ToList()
        };
        
        // 11. 廣播座位狀態更新 (SignalR)
        try
        {
            var roomName = $"showtime_{request.ShowTimeId}";
            await _hubContext.Clients.Group(roomName).SendAsync("SeatStatusChanged", new
            {
                showtimeId = request.ShowTimeId,
                seatIds = request.SeatIds,
                status = "sold" // 在前台視為鎖定
            });
        }
        catch (Exception)
        {
            // 廣播失敗不應影響訂單建立
            // TODO: Log error
        }
        
        return response;
    }

    /// <summary>
    /// 生成唯一訂單編號（格式：#ABC-12345）
    /// </summary>
    private async Task<string> GenerateUniqueOrderNumberAsync()
    {
        const int maxRetries = 5;
        var random = new Random();

        for (int i = 0; i < maxRetries; i++)
        {
            // 生成三個隨機大寫字母
            var letters = string.Concat(Enumerable.Range(0, 3)
                .Select(_ => (char)random.Next('A', 'Z' + 1)));

            // 生成五位數字
            var numbers = random.Next(10000, 100000);

            var orderNumber = $"#{letters}-{numbers}";

            // 檢查是否已存在
            var exists = await _orderRepository.OrderNumberExistsAsync(orderNumber);
            if (!exists)
            {
                return orderNumber;
            }
        }

        throw new InvalidOperationException("無法生成唯一訂單編號，請稍後再試");
    }

    /// <summary>
    /// 生成唯一票券編號（8碼數字）
    /// </summary>
    private async Task<string> GenerateUniqueTicketNumberAsync()
    {
        const int maxRetries = 5;
        var random = new Random();

        for (int i = 0; i < maxRetries; i++)
        {
            // 生成 8 碼數字（10000000 - 99999999）
            var ticketNumber = random.Next(10000000, 100000000).ToString();

            // 檢查是否已存在
            var exists = await _ticketRepository.TicketNumberExistsAsync(ticketNumber);
            if (!exists)
            {
                return ticketNumber;
            }
        }

        throw new InvalidOperationException("無法生成唯一票券編號，請稍後再試");
    }

    /// <summary>
    /// 根據影廳類型計算票價
    /// </summary>
    /// <param name="theaterType">影廳類型</param>
    /// <returns>票價</returns>
    private decimal CalculateTicketPrice(string theaterType)
    {
        return theaterType switch
        {
            "Digital" => 300m,
            "4DX" => 380m,
            "IMAX" => 380m,
            _ => throw new InvalidOperationException($"未知的影廳類型: {theaterType}")
        };
    }

    /// <inheritdoc/>
    public async Task<OrderDetailResponseDto?> GetOrderDetailAsync(int orderId, int userId)
    {
        // 取得完整訂單資料
        var order = await _orderRepository.GetByIdAsync(orderId);

        //檢查訂單是否存在
        if (order == null)
        {
            return null;
        }

        // 權限驗證：使用者只能查看自己的訂單
        if (order.UserId != userId)
        {
            return null;
        }

        // 組裝 DTO
        var response = new OrderDetailResponseDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            ExpiresAt = order.ExpiresAt,
            Movie = new OrderMovieInfoDto
            {
                Title = order.ShowTime.Movie.Title,
                Rating = order.ShowTime.Movie.Rating,
                Duration = order.ShowTime.Movie.Duration,
                PosterUrl = order.ShowTime.Movie.PosterUrl
            },
            Showtime = new OrderShowtimeInfoDto
            {
                Date = order.ShowTime.ShowDate.ToString("yyyy-MM-dd"),
                StartTime = order.ShowTime.StartTime.ToString(@"hh\:mm"),
                DayOfWeek = GetChineseDayOfWeek(order.ShowTime.ShowDate)
            },
            Theater = new OrderTheaterInfoDto
            {
                Name = order.ShowTime.Theater.Name,
                Type = order.ShowTime.Theater.Type
            },
            Seats = order.Tickets.Select(t => new SeatInfoDto
            {
                SeatId = t.Seat.Id,
                RowName = t.Seat.RowName,
                ColumnNumber = t.Seat.ColumnNumber,
                TicketNumber = t.TicketNumber
            }).ToList(),
            TotalAmount = order.TotalPrice
        };

        return response;
    }

    /// <summary>
    /// 取得中文星期幾
    /// </summary>
    private string GetChineseDayOfWeek(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => "日",
            DayOfWeek.Monday => "一",
            DayOfWeek.Tuesday => "二",
            DayOfWeek.Wednesday => "三",
            DayOfWeek.Thursday => "四",
            DayOfWeek.Friday => "五",
            DayOfWeek.Saturday => "六",
            _ => ""
        };
    }
}
