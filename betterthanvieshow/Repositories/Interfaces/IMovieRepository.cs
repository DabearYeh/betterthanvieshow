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

    /// <summary>
    /// 取得所有電影
    /// </summary>
    /// <returns>電影列表</returns>
    Task<List<Movie>> GetAllAsync();

    /// <summary>
    /// 取得輪播電影（CanCarousel = true）
    /// </summary>
    /// <returns>輪播電影列表</returns>
    Task<List<Movie>> GetCarouselMoviesAsync();

    /// <summary>
    /// 取得即將上映電影（ReleaseDate > 今天）
    /// </summary>
    /// <returns>即將上映電影列表</returns>
    Task<List<Movie>> GetComingSoonMoviesAsync();

    /// <summary>
    /// 取得正在上映電影（今天在 [ReleaseDate, EndDate] 範圍內）
    /// </summary>
    /// <returns>正在上映電影列表</returns>
    Task<List<Movie>> GetMoviesOnSaleAsync();

    /// <summary>
    /// 取得最新建立的正在上映電影（按建立時間降序）
    /// </summary>
    /// <param name="count">取得數量</param>
    /// <returns>最新建立的正在上映電影列表</returns>
    Task<List<Movie>> GetRecentOnSaleMoviesAsync(int count);

    /// <summary>
    /// 搜尋電影（根據關鍵字搜尋標題）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <returns>符合條件的電影列表</returns>
    Task<List<Movie>> SearchMoviesAsync(string keyword);
}
