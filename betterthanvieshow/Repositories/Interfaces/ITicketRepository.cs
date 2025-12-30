using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 票券 Repository 介面
/// </summary>
public interface ITicketRepository
{
    /// <summary>
    /// 取得指定場次已售出的票券數量（狀態為 待支付、未使用、已使用）
    /// </summary>
    /// <param name="showTimeId">場次 ID</param>
    /// <returns>已售出票券數</returns>
    Task<int> GetSoldTicketCountByShowTimeAsync(int showTimeId);

    /// <summary>
    /// 取得指定場次已售出的座位 ID 集合（狀態為 待支付、未使用、已使用）
    /// </summary>
    /// <param name="showTimeId">場次 ID</param>
    /// <returns>已售出的座位 ID 集合</returns>
    Task<HashSet<int>> GetSoldSeatIdsByShowTimeAsync(int showTimeId);

    /// <summary>
    /// 批次創建票券
    /// </summary>
    /// <param name="tickets">票券列表</param>
    /// <returns>創建後的票券列表</returns>
    Task<List<Ticket>> CreateBatchAsync(List<Ticket> tickets);

    /// <summary>
    /// 檢查座位是否已被訂購（有效票券狀態：待支付、未使用、已使用）
    /// </summary>
    /// <param name="showTimeId">場次 ID</param>
    /// <param name="seatId">座位 ID</param>
    /// <returns>true 表示已被訂購，false 表示未被訂購</returns>
    Task<bool> IsSeatOccupiedAsync(int showTimeId, int seatId);

    /// <summary>
    /// 檢查票券編號是否存在
    /// </summary>
    /// <param name="ticketNumber">票券編號</param>
    /// <returns>true 表示存在，false 表示不存在</returns>
    Task<bool> TicketNumberExistsAsync(string ticketNumber);

    /// <summary>
    /// 根據訂單 ID 取得所有票券
    /// </summary>
    /// <param name="orderId">訂單 ID</param>
    /// <returns>票券列表</returns>
    Task<List<Ticket>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// 更新票券資訊
    /// </summary>
    /// <param name="ticket">票券實體</param>
    Task<Ticket> UpdateAsync(Ticket ticket);

    /// <summary>
    /// 根據票券 ID 取得座位資訊
    /// </summary>
    /// <param name="ticketId">票券 ID</param>
    /// <returns>座位實體（可能為 null）</returns>
    Task<Seat?> GetSeatByTicketIdAsync(int ticketId);

    /// <summary>
    /// 根據票券編號查詢票券及完整關聯資料（包含 Seat, ShowTime, Movie, Theater）
    /// </summary>
    /// <param name="ticketNumber">票券編號</param>
    /// <returns>票券實體（可能為 null）</returns>
    Task<Ticket?> GetByTicketNumberWithDetailsAsync(string ticketNumber);

    /// <summary>
    /// 根據票券 ID 取得票券（不含關聯資料）
    /// </summary>
    /// <param name="ticketId">票券 ID</param>
    /// <returns>票券實體（可能為 null）</returns>
    Task<Ticket?> GetByIdAsync(int ticketId);
}
