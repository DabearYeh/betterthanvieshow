using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 場次控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;
    private readonly ILogger<ShowtimesController> _logger;

    public ShowtimesController(
        IShowtimeService showtimeService,
        ILogger<ShowtimesController> logger)
    {
        _showtimeService = showtimeService;
        _logger = logger;
    }

    /// <summary>
    /// /api/showtimes/{id}/seats 取得場次的座位配置
    /// </summary>
    /// <remarks>
    /// 此端點用於訂票流程的第三步：選擇座位。
    /// 
    /// 返回該場次的完整座位資訊，包含座位二維陣列和每個座位的狀態。
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// 
    /// **業務規則**：
    /// - 座位狀態分為：可用 (available)、已售 (sold)、走道 (aisle)、空位 (empty)、無效 (invalid)
    /// - **已售 (sold)**：該座位已有有效票券（待支付、未使用、已使用）
    /// - 走道/空位/無效：不可選擇
    /// - 已售出座位包含「待支付」狀態的票券，表示該座位已被鎖定
    /// 
    /// **WebSocket 整合**：
    /// - 前端在進入此頁面後應連接到 SignalR Hub (`/hub/showtime`)。
    /// - 連線後呼叫 `JoinShowtime(showtimeId)` 加入該場次的即時更新群組。
    /// - 當座位狀態變更（有人訂位或訂位過期釋放）時，會收到 `SeatStatusChanged` 事件。
    /// 
    /// **事件資料格式**：
    /// ```json
    /// {
    ///   "showtimeId": 7,
    ///   "seatIds": [1, 2],
    ///   "status": "sold"  // 或 "available"
    /// }
    /// ```
    /// 
    /// **回應資料包含**：
    /// - 場次和電影資訊
    /// - 影廳資訊和票價
    /// - 座位二維陣列（完整配置）
    /// </remarks>
    /// <param name="id">場次 ID</param>
    /// <response code="200">成功取得座位配置</response>
    /// <response code="404">找不到指定的場次</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>座位配置</returns>
    [HttpGet("{id}/seats")]
    [AllowAnonymous]
    [Tags("Booking - 訂票流程")]
    [ProducesResponseType(typeof(ApiResponse<ShowtimeSeatsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetShowtimeSeats(int id)
    {
        try
        {
            var result = await _showtimeService.GetShowtimeSeatsAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"找不到 ID 為 {id} 的場次",
                    Data = null
                });
            }

            return Ok(new ApiResponse<ShowtimeSeatsResponseDto>
            {
                Success = true,
                Message = "成功取得座位配置",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得場次 {ShowtimeId} 的座位配置時發生錯誤", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "取得座位配置時發生錯誤",
                Data = null
            });
        }
    }
}
