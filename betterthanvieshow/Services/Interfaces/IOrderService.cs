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
}
