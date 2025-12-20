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
            // 驗證座位陣列尺寸
            if (request.Seats.Count != request.RowCount)
            {
                return ApiResponse<TheaterResponseDto>.FailureResponse(
                    $"座位陣列排數 ({request.Seats.Count}) 與 RowCount ({request.RowCount}) 不符"
                );
            }

            foreach (var row in request.Seats)
            {
                if (row.Count != request.ColumnCount)
                {
                    return ApiResponse<TheaterResponseDto>.FailureResponse(
                        $"座位陣列列數與 ColumnCount ({request.ColumnCount}) 不符"
                    );
                }
            }

            // 先計算 TotalSeats（一般座位 + 殘障座位）
            int actualSeatCount = 0;
            for (int row = 0; row < request.RowCount; row++)
            {
                for (int col = 0; col < request.ColumnCount; col++)
                {
                    string seatType = request.Seats[row][col];
                    if (seatType == "一般座位" || seatType == "殘障座位")
                    {
                        actualSeatCount++;
                    }
                }
            }

            // 驗證座位數量必須大於 0
            if (actualSeatCount == 0)
            {
                return ApiResponse<TheaterResponseDto>.FailureResponse(
                    "影廳必須至少包含一個可販售座位（一般座位或殘障座位）"
                );
            }

            // 建立 Theater 實體（直接設定計算後的 TotalSeats）
            var theater = new Theater
            {
                Name = request.Name,
                Type = request.Type,
                Floor = request.Floor,
                RowCount = request.RowCount,
                ColumnCount = request.ColumnCount,
                TotalSeats = actualSeatCount  // 直接設定正確的座位數
            };

            // 儲存 Theater 到資料庫以取得 ID
            var createdTheater = await _theaterRepository.CreateAsync(theater);

            // 建立座位列表
            var seats = new List<Seat>();

            for (int row = 0; row < request.RowCount; row++)
            {
                string rowName = ((char)('A' + row)).ToString(); // A, B, C...

                for (int col = 0; col < request.ColumnCount; col++)
                {
                    string seatType = request.Seats[row][col];
                    
                    var seat = new Seat
                    {
                        TheaterId = createdTheater.Id,
                        RowName = rowName,
                        ColumnNumber = col + 1,  // 1, 2, 3...
                        SeatType = seatType,
                        IsValid = seatType != "Empty"
                    };

                    seats.Add(seat);
                }
            }

            // 使用 DbContext 批次新增座位（不需要再更新 TotalSeats）
            await _theaterRepository.CreateSeatsOnlyAsync(createdTheater.Id, seats);

            // 重新載入 Theater 以取得更新後的 TotalSeats
            var updatedTheater = await _theaterRepository.GetByIdAsync(createdTheater.Id);

            // 轉換為 DTO
            var theaterDto = new TheaterResponseDto
            {
                Id = updatedTheater.Id,
                Name = updatedTheater.Name,
                Type = updatedTheater.Type,
                Floor = updatedTheater.Floor,
                RowCount = updatedTheater.RowCount,
                ColumnCount = updatedTheater.ColumnCount,
                TotalSeats = updatedTheater.TotalSeats
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

    /// <summary>
    /// 刪除影廳
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>刪除結果</returns>
    public async Task<ApiResponse<object>> DeleteTheaterAsync(int id)
    {
        try
        {
            // 檢查影廳是否存在
            var exists = await _theaterRepository.ExistsAsync(id);
            if (!exists)
            {
                return ApiResponse<object>.FailureResponse(
                    "找不到指定的影廳"
                );
            }

            // TODO: 未來需要檢查是否有關聯的場次 (MovieShowTime)
            // 當 MovieShowTime 實體建立後，添加以下檢查：
            // var hasShowtimes = await _showtimeRepository.HasTheaterShowtimesAsync(id);
            // if (hasShowtimes)
            // {
            //     return ApiResponse<object>.FailureResponse(
            //         "影廳目前有場次安排，無法刪除"
            //     );
            // }

            // 刪除影廳及其座位
            await _theaterRepository.DeleteAsync(id);

            return ApiResponse<object>.SuccessResponse(
                null,
                "影廳刪除成功"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除影廳時發生錯誤，影廳 ID: {TheaterId}", id);
            return ApiResponse<object>.FailureResponse(
                "刪除影廳失敗，請稍後再試"
            );
        }
    }
}
