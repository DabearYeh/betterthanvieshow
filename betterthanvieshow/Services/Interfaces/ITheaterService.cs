using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 影廳服務介面
/// </summary>
public interface ITheaterService
{
    /// <summary>
    /// 取得所有影廳
    /// </summary>
    /// <returns>影廳列表回應</returns>
    Task<ApiResponse<List<TheaterResponseDto>>> GetAllTheatersAsync();

    /// <summary>
    /// 建立新影廳
    /// </summary>
    /// <param name="request">建立影廳請求</param>
    /// <returns>建立結果</returns>
    Task<ApiResponse<TheaterResponseDto>> CreateTheaterAsync(CreateTheaterRequestDto request);
}
