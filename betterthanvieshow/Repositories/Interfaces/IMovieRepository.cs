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
}
