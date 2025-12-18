using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 影廳服務實作
/// </summary>
public class TheaterService : ITheaterService
{
    private readonly ITheaterRepository _theaterRepository;
    private readonly ILogger<TheaterService> _logger;

    public TheaterService(
        ITheaterRepository theaterRepository,
        ILogger<TheaterService> logger)
    {
        _theaterRepository = theaterRepository;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有影廳
    /// </summary>
    /// <returns>影廳列表回應</returns>
    public async Task<ApiResponse<List<TheaterResponseDto>>> GetAllTheatersAsync()
    {
        try
        {
            var theaters = await _theaterRepository.GetAllAsync();

            var theaterDtos = theaters.Select(t => new TheaterResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Type = t.Type,
                Floor = t.Floor,
                RowCount = t.RowCount,
                ColumnCount = t.ColumnCount,
                TotalSeats = t.TotalSeats
            }).ToList();

            return ApiResponse<List<TheaterResponseDto>>.SuccessResponse(
                theaterDtos,
                "查詢成功"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得影廳列表時發生錯誤");
            return ApiResponse<List<TheaterResponseDto>>.FailureResponse(
                "查詢影廳列表失敗，請稍後再試"
            );
        }
    }
}
