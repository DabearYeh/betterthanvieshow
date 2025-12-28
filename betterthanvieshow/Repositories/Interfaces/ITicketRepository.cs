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
}
