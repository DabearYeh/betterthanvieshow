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

    /// <summary>
    /// 建立新影廳
    /// </summary>
    /// <param name="theater">影廳實體</param>
    /// <returns>建立成功的影廳實體</returns>
    Task<Theater> CreateAsync(Theater theater);
}
