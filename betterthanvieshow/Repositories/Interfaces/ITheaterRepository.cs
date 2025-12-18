using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 影廳資料存取介面
/// </summary>
public interface ITheaterRepository
{
    /// <summary>
    /// 取得所有影廳
    /// </summary>
    /// <returns>影廳實體列表</returns>
    Task<List<Theater>> GetAllAsync();
}
