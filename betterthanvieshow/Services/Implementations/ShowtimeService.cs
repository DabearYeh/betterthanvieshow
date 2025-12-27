using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 場次 Service 實作
/// </summary>
public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<ShowtimeService> _logger;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        ISeatRepository seatRepository,
        ITicketRepository ticketRepository,
        ILogger<ShowtimeService> logger)
    {
        _showtimeRepository = showtimeRepository;
        _seatRepository = seatRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ShowtimeSeatsResponseDto?> GetShowtimeSeatsAsync(int showtimeId)
    {
        try
        {
            _logger.LogInformation("開始取得場次 {ShowtimeId} 的座位配置", showtimeId);

            // 1. 取得場次資訊（包含電影和影廳）
            var showtime = await _showtimeRepository.GetByIdWithDetailsAsync(showtimeId);
            if (showtime == null)
            {
                _logger.LogWarning("找不到場次: ID={ShowtimeId}", showtimeId);
                return null;
            }

            // 2. 取得影廳的所有座位
            var seats = await _seatRepository.GetSeatsByTheaterIdAsync(showtime.TheaterId);

            // 3. 查詢該場次的所有有效票券（用於判斷座位狀態）
            var soldSeatIds = await _ticketRepository.GetSoldSeatIdsByShowTimeAsync(showtimeId);

            // 4. 建立座位二維陣列
            var seatGrid = BuildSeatGrid(seats, soldSeatIds, showtime.Theater.RowCount, showtime.Theater.ColumnCount);

            _logger.LogInformation("成功取得場次 {ShowtimeId} 的座位配置", showtimeId);

            // 5. 計算結束時間和票價
            var endTime = showtime.StartTime.Add(TimeSpan.FromMinutes(showtime.Movie.Duration));
            var price = GetPriceByTheaterType(showtime.Theater.Type);

            return new ShowtimeSeatsResponseDto
            {
                ShowTimeId = showtime.Id,
                MovieTitle = showtime.Movie.Title,
                ShowDate = showtime.ShowDate.ToString("yyyy-MM-dd"),
                StartTime = $"{showtime.StartTime.Hours:D2}:{showtime.StartTime.Minutes:D2}",
                EndTime = $"{endTime.Hours:D2}:{endTime.Minutes:D2}",
                TheaterName = showtime.Theater.Name,
                TheaterType = showtime.Theater.Type,
                Price = price,
                RowCount = showtime.Theater.RowCount,
                ColumnCount = showtime.Theater.ColumnCount,
                Seats = seatGrid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得場次 {ShowtimeId} 的座位配置時發生錯誤", showtimeId);
            throw;
        }
    }

    /// <summary>
    /// 建立座位二維陣列
    /// </summary>
    private static List<List<ShowtimeSeatDto>> BuildSeatGrid(
        List<Seat> seats, 
        HashSet<int> soldSeatIds, 
        int rowCount, 
        int columnCount)
    {
        var grid = new List<List<ShowtimeSeatDto>>();

        // 建立索引以快速查找座位
        var seatMap = seats.ToDictionary(s => (s.RowName, s.ColumnNumber), s => s);

        // 生成排名列表 (A, B, C, ...)
        var rowNames = seats.Select(s => s.RowName).Distinct().OrderBy(r => r).ToList();

        foreach (var rowName in rowNames)
        {
            var row = new List<ShowtimeSeatDto>();

            for (int col = 1; col <= columnCount; col++)
            {
                if (seatMap.TryGetValue((rowName, col), out var seat))
                {
                    // 判斷座位狀態
                    string status;
                    if (seat.SeatType == "走道")
                        status = "aisle";
                    else if (seat.SeatType == "Empty")
                        status = "empty";
                    else if (!seat.IsValid)
                        status = "invalid";
                    else if (soldSeatIds.Contains(seat.Id))
                        status = "sold";
                    else
                        status = "available";

                    row.Add(new ShowtimeSeatDto
                    {
                        SeatId = seat.Id,
                        RowName = seat.RowName,
                        ColumnNumber = seat.ColumnNumber,
                        SeatType = seat.SeatType,
                        Status = status,
                        IsValid = seat.IsValid
                    });
                }
                else
                {
                    // 不存在的位置填入 empty
                    row.Add(new ShowtimeSeatDto
                    {
                        SeatId = 0,
                        RowName = rowName,
                        ColumnNumber = col,
                        SeatType = "Empty",
                        Status = "empty",
                        IsValid = false
                    });
                }
            }

            grid.Add(row);
        }

        return grid;
    }

    /// <summary>
    /// 根據影廳類型取得票價
    /// </summary>
    private static int GetPriceByTheaterType(string theaterType)
    {
        return theaterType switch
        {
            "一般數位" => 300,
            "4DX" => 380,
            "IMAX" => 380,
            _ => 300
        };
    }
}
