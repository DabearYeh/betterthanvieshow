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
}
