using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 電影服務介面
/// </summary>
public interface IMovieService
{
    /// <summary>
    /// 建立新電影
    /// </summary>
    /// <param name="request">建立電影請求</param>
    /// <returns>建立結果</returns>
    Task<ApiResponse<MovieResponseDto>> CreateMovieAsync(CreateMovieRequestDto request);

    /// <summary>
    /// 更新電影
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <param name="request">更新電影請求</param>
    /// <returns>更新結果</returns>
    Task<ApiResponse<MovieResponseDto>> UpdateMovieAsync(int id, UpdateMovieRequestDto request);

    /// <summary>
    /// 取得所有電影
    /// </summary>
    /// <returns>電影列表</returns>
    Task<ApiResponse<List<MovieListItemDto>>> GetAllMoviesAsync();

    /// <summary>
    /// 取得單一電影詳情
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <returns>電影詳情</returns>
    Task<ApiResponse<MovieResponseDto>> GetMovieByIdAsync(int id);

    /// <summary>
    /// 取得首頁電影資料（輪播、本週前10、即將上映、隨機推薦、所有電影）
    /// </summary>
    /// <returns>首頁電影資料</returns>
    Task<ApiResponse<HomepageMoviesResponseDto>> GetHomepageMoviesAsync();

    /// <summary>
    /// 搜尋電影
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <returns>搜尋結果</returns>
    Task<ApiResponse<List<MovieSimpleDto>>> SearchMoviesAsync(string keyword);

    /// <summary>
    /// 取得指定電影的可訂票日期
    /// </summary>
    /// <param name="movieId">電影 ID</param>
    /// <returns>可訂票日期的回應 DTO</returns>
    Task<MovieAvailableDatesResponseDto?> GetAvailableDatesAsync(int movieId);
}
