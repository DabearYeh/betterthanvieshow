using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
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

    /// <summary>
    /// 建立新影廳
    /// </summary>
    /// <param name="request">建立影廳請求</param>
    /// <returns>建立結果</returns>
    public async Task<ApiResponse<TheaterResponseDto>> CreateTheaterAsync(CreateTheaterRequestDto request)
    {
        try
        {
            // 建立 Theater 實體
            var theater = new Theater
            {
                Name = request.Name,
                Type = request.Type,
                Floor = request.Floor,
                RowCount = request.RowCount,
                ColumnCount = request.ColumnCount,
                TotalSeats = request.RowCount * request.ColumnCount  // 初始座位總數
            };

            // 儲存到資料庫
            var createdTheater = await _theaterRepository.CreateAsync(theater);

            // 轉換為 DTO
            var theaterDto = new TheaterResponseDto
            {
                Id = createdTheater.Id,
                Name = createdTheater.Name,
                Type = createdTheater.Type,
                Floor = createdTheater.Floor,
                RowCount = createdTheater.RowCount,
                ColumnCount = createdTheater.ColumnCount,
                TotalSeats = createdTheater.TotalSeats
            };

            return ApiResponse<TheaterResponseDto>.SuccessResponse(
                theaterDto,
                "影廳建立成功"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立影廳時發生錯誤");
            return ApiResponse<TheaterResponseDto>.FailureResponse(
                "建立影廳失敗，請稍後再試"
            );
        }
    }
}
