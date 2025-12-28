using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 訂單資料存取介面
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// 創建訂單
    /// </summary>
    /// <param name="order">訂單實體</param>
    /// <returns>創建後的訂單實體</returns>
    Task<Order> CreateAsync(Order order);

    /// <summary>
    /// 根據 ID 查詢訂單
    /// </summary>
    /// <param name="id">訂單 ID</param>
    /// <returns>訂單實體，若不存在則為 null</returns>
    Task<Order?> GetByIdAsync(int id);

    /// <summary>
    /// 根據訂單編號查詢訂單
    /// </summary>
    /// <param name="orderNumber">訂單編號</param>
    /// <returns>訂單實體，若不存在則為 null</returns>
    Task<Order?> GetByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// 檢查訂單編號是否存在
    /// </summary>
    /// <param name="orderNumber">訂單編號</param>
    /// <returns>true 表示存在，false 表示不存在</returns>
    Task<bool> OrderNumberExistsAsync(string orderNumber);
}
