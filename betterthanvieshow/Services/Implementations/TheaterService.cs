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

            // 先計算 TotalSeats（只有 Standard 和 Wheelchair 算入總座位數）
            int actualSeatCount = 0;
            for (int row = 0; row < request.RowCount; row++)
            {
                for (int col = 0; col < request.ColumnCount; col++)
                {
                    string seatType = request.Seats[row][col];
                    // 使用英文代碼判斷
                    if (seatType == "Standard" || seatType == "Wheelchair")
                    {
                        actualSeatCount++;
                    }
                }
            }

            // 驗證座位數量必須大於 0
            if (actualSeatCount == 0)
            {
                return ApiResponse<TheaterResponseDto>.FailureResponse(
                    "影廳必須至少包含一個可販售座位（Standard 或 Wheelchair）"
                );
            }

            // 建立 Theater 實體
            var theater = new Theater
            {
                Name = request.Name,
                Type = request.Type,
                Floor = request.Floor,
                RowCount = request.RowCount,
                ColumnCount = request.ColumnCount,
                TotalSeats = actualSeatCount
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
                        ColumnNumber = col + 1,
                        SeatType = seatType, // 直接存入英文代碼
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

            // 檢查是否有關聯的場次 (MovieShowTime)
            var hasShowtimes = await _theaterRepository.HasShowtimesAsync(id);
            if (hasShowtimes)
            {
                return ApiResponse<object>.FailureResponse(
                    "影廳目前有場次安排，無法刪除"
                );
            }

            // 刪除影廳及其座位
            await _theaterRepository.DeleteAsync(id);

            return ApiResponse<object>.SuccessResponse(
                new object(),
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

    /// <summary>
    /// 根據 ID 取得影廳詳細資訊（含座位表）
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>影廳詳細資訊回應</returns>
    public async Task<ApiResponse<TheaterDetailResponseDto>> GetTheaterByIdAsync(int id)
    {
        try
        {
            var theater = await _theaterRepository.GetByIdWithSeatsAsync(id);
            
            if (theater == null)
            {
                return ApiResponse<TheaterDetailResponseDto>.FailureResponse(
                    "找不到指定的影廳"
                );
            }

            // 將座位轉換為二維陣列格式
            var seatMap = new List<List<SeatDto>>();
            var seats = theater.Seats.ToList();

            for (int row = 0; row < theater.RowCount; row++)
            {
                var rowSeats = new List<SeatDto>();
                string rowName = ((char)('A' + row)).ToString();

                for (int col = 1; col <= theater.ColumnCount; col++)
                {
                    var seat = seats.FirstOrDefault(s => 
                        s.RowName == rowName && s.ColumnNumber == col);

                    rowSeats.Add(new SeatDto
                    {
                        RowName = rowName,
                        ColumnNumber = col,
                        SeatType = seat?.SeatType ?? "Empty" // 直接回傳英文代碼
                    });
                }
                seatMap.Add(rowSeats);
            }

            var theaterDto = new TheaterDetailResponseDto
            {
                Id = theater.Id,
                Name = theater.Name,
                RowCount = theater.RowCount,
                ColumnCount = theater.ColumnCount,
                SeatMap = seatMap
            };

            return ApiResponse<TheaterDetailResponseDto>.SuccessResponse(
                theaterDto,
                "查詢成功"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢影廳時發生錯誤，影廳 ID: {TheaterId}", id);
            return ApiResponse<TheaterDetailResponseDto>.FailureResponse(
                "查詢影廳失敗，請稍後再試"
            );
        }
    }
}
