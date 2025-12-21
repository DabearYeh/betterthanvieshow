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
}
