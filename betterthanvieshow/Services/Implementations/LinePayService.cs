using betterthanvieshow.Infrastructure.LinePay;
using betterthanvieshow.Models.DTOs.Payment;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// LINE Pay 付款服務實作
/// </summary>
public class LinePayService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly LinePayHttpClient _linePayClient;
    private readonly LinePayOptions _linePayOptions;

    public LinePayService(
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        IShowtimeRepository showtimeRepository,
        LinePayHttpClient linePayClient,
        IOptions<LinePayOptions> linePayOptions)
    {
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _showtimeRepository = showtimeRepository;
        _linePayClient = linePayClient;
        _linePayOptions = linePayOptions.Value;
    }

    /// <inheritdoc/>
    public async Task<PaymentRequestResponseDto> CreatePaymentRequestAsync(int orderId, int userId)
    {
        // 1. 查詢訂單資訊（包含票券、場次、影廳）
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"訂單 ID {orderId} 不存在");
        }

        // 2. 驗證訂單所屬使用者
        if (order.UserId != userId)
        {
            throw new UnauthorizedAccessException("無權訪問此訂單");
        }

        // 3. 驗證訂單狀態（必須為 Pending）
        if (order.Status != "Pending")
        {
            throw new InvalidOperationException($"訂單狀態錯誤：{order.Status}，僅 Pending 狀態可發起付款");
        }

        // 4. 驗證訂單是否已過期
        if (order.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("訂單已過期，無法進行付款");
        }

        // 5. 查詢票券資訊（用於組裝商品列表）
        var tickets = await _ticketRepository.GetByOrderIdAsync(orderId);
        if (tickets.Count == 0)
        {
            throw new InvalidOperationException("訂單無票券資料");
        }

        // 6. 查詢場次資訊（用於商品描述）
        var showtime = await _showtimeRepository.GetByIdWithDetailsAsync(order.ShowTimeId);
        if (showtime == null)
        {
            throw new InvalidOperationException("場次資料不存在");
        }

        // 7. 組裝 LINE Pay Request API 參數
        var requestBody = new LinePayRequestBody
        {
            Amount = (int)order.TotalPrice,  // 轉換為整數
            Currency = "TWD",
            OrderId = order.OrderNumber, // 使用 OrderNumber 作為 LINE Pay 的訂單識別
            Packages = new List<LinePayPackage>
            {
                new LinePayPackage
                {
                    Id = "1",
                    Amount = (int)order.TotalPrice,  // 轉換為整數
                    Products = new List<LinePayProduct>
                    {
                        new LinePayProduct
                        {
                            Name = $"{showtime.Movie.Title} - {showtime.Theater.Name}",
                            Quantity = tickets.Count,
                            Price = (int)tickets.First().Price  // 轉換為整數
                        }
                    }
                }
            },
            RedirectUrls = new LinePayRedirectUrls
            {
                ConfirmUrl = _linePayOptions.GetConfirmUrl(),
                CancelUrl = _linePayOptions.GetCancelUrl()
            }
        };

        // 8. 呼叫 LINE Pay Request API
        var response = await _linePayClient.PostAsync<LinePayApiResponse<LinePayRequestInfo>>(
            "/v4/payments/request",
            requestBody
        );

        // 9. 驗證回應
        if (!response.IsSuccess || response.Info == null)
        {
            throw new InvalidOperationException(
                $"LINE Pay 付款請求失敗：[{response.ReturnCode}] {response.ReturnMessage}"
            );
        }

        // 10. 回傳付款網址與交易 ID
        return new PaymentRequestResponseDto
        {
            PaymentUrl = response.Info.PaymentUrl.Web,
            TransactionId = response.Info.TransactionId
        };
    }

    /// <inheritdoc/>
    public async Task<PaymentConfirmResponseDto> ConfirmPaymentAsync(long transactionId, int orderId, int userId)
    {
        // 1. 查詢訂單資訊
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"訂單 ID {orderId} 不存在");
        }

        // 2. 驗證訂單所屬使用者
        if (order.UserId != userId)
        {
            throw new UnauthorizedAccessException("無權訪問此訂單");
        }

        // 3. 驗證訂單狀態（必須為 Pending）
        if (order.Status != "Pending")
        {
            throw new InvalidOperationException($"訂單狀態錯誤：{order.Status}，僅 Pending 狀態可確認付款");
        }

        // 4. 查詢票券資訊
        var tickets = await _ticketRepository.GetByOrderIdAsync(orderId);
        if (tickets.Count == 0)
        {
            throw new InvalidOperationException("訂單無票券資料");
        }

        // 5. 組裝 LINE Pay Confirm API 參數
        var confirmBody = new LinePayConfirmBody
        {
            Amount = (int)order.TotalPrice,  // 轉換為整數
            Currency = "TWD"
        };

        // 6. 呼叫 LINE Pay Confirm API
        var response = await _linePayClient.PostAsync<LinePayApiResponse<LinePayConfirmInfo>>(
            $"/v4/payments/{transactionId}/confirm",
            confirmBody
        );

        // 7. 驗證回應（ReturnCode 必須為 "0000"）
        if (!response.IsSuccess || response.Info == null)
        {
            throw new InvalidOperationException(
                $"LINE Pay 付款確認失敗：[{response.ReturnCode}] {response.ReturnMessage}"
            );
        }

        // 8. 更新資料庫
        // 8.1 更新訂單狀態
        order.Status = "Paid";
        order.PaymentTransactionId = transactionId.ToString();
        await _orderRepository.UpdateAsync(order);

        // 8.2 更新票券狀態 & 生成 QR Code
        foreach (var ticket in tickets)
        {
            ticket.Status = "Unused";
            ticket.QrCode = GenerateTicketQrCode(ticket.TicketNumber);
            await _ticketRepository.UpdateAsync(ticket);
        }

        // 9. 查詢座位資訊（用於回傳）
        var ticketDtos = new List<TicketDto>();
        foreach (var ticket in tickets)
        {
            var seat = await _ticketRepository.GetSeatByTicketIdAsync(ticket.Id);
            ticketDtos.Add(new TicketDto
            {
                TicketNumber = ticket.TicketNumber,
                QrCode = ticket.QrCode ?? string.Empty,
                SeatInfo = seat != null ? $"{seat.RowName}{seat.ColumnNumber}" : "未知"
            });
        }

        // 10. 回傳付款成功訊息與票券資訊
        return new PaymentConfirmResponseDto
        {
            Success = true,
            OrderNumber = order.OrderNumber,
            Tickets = ticketDtos
        };
    }

    /// <summary>
    /// 生成票券 QR Code (暫時使用票券編號，未來可串接 QR Code 生成服務)
    /// </summary>
    private string GenerateTicketQrCode(string ticketNumber)
    {
        // TODO: 整合真實的 QR Code 生成服務
        // 目前暫時回傳 Base64 格式的文字作為 placeholder
        var qrContent = $"TICKET:{ticketNumber}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(qrContent);
        return Convert.ToBase64String(bytes);
    }
}
