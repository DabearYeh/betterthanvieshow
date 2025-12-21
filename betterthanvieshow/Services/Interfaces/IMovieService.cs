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
}
