using betterthanvieshow.Models.DTOs.Payment;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// LINE Pay 付款服務介面
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// 發起 LINE Pay 付款請求 (v4)
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>付款請求結果 (含 paymentUrl 與 transactionId)</returns>
    Task<PaymentRequestResponseDto> CreatePaymentRequestAsync(int orderId, int userId);

    /// <summary>
    /// 確認 LINE Pay 付款完成 (v4)
    /// </summary>
    /// <param name="transactionId">LINE Pay 交易 ID</param>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="userId">使用者 ID</param>
    /// <returns>付款確認結果 (含票券資訊)</returns>
    Task<PaymentConfirmResponseDto> ConfirmPaymentAsync(long transactionId, int orderId, int userId);
}
