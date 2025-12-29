using betterthanvieshow.Models.DTOs;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 訂單服務介面
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// 創建訂單
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="request">創建訂單請求</param>
    /// <returns>創建訂單回應</returns>
    Task<CreateOrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequestDto request);

    /// <summary>
    /// 取得訂單詳情
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <param name="userId">使用者 ID（用於權限驗證）</param>
    /// <returns>訂單詳情，若訂單不存在或無權查看則為 null</returns>
    Task<OrderDetailResponseDto?> GetOrderDetailAsync(int orderId, int userId);
}
