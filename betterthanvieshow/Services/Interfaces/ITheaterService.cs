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
}
