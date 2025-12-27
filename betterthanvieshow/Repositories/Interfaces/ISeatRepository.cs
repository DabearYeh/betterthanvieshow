using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 座位 Repository 介面
/// </summary>
public interface ISeatRepository
{
    /// <summary>
    /// 根據影廳 ID 取得所有座位
    /// </summary>
    /// <param name="theaterId">影廳 ID</param>
    /// <returns>座位列表</returns>
    Task<List<Seat>> GetSeatsByTheaterIdAsync(int theaterId);
}
