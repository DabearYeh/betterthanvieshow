using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 電影資料存取介面
/// </summary>
public interface IMovieRepository
{
    /// <summary>
    /// 建立新電影
    /// </summary>
    /// <param name="movie">電影實體</param>
    /// <returns>建立成功的電影實體</returns>
    Task<Movie> CreateAsync(Movie movie);

    /// <summary>
    /// 根據 ID 取得電影
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <returns>電影實體，若不存在回傳 null</returns>
    Task<Movie?> GetByIdAsync(int id);

    /// <summary>
    /// 更新電影
    /// </summary>
    /// <param name="movie">電影實體</param>
    /// <returns>更新後的電影實體</returns>
    Task<Movie> UpdateAsync(Movie movie);
}
