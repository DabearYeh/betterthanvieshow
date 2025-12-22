using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 每日時刻表管理 API
/// </summary>
[ApiController]
[Route("api/admin/daily-schedules")]
[Authorize(Roles = "Admin")]
public class DailySchedulesController : ControllerBase
{
    private readonly IDailyScheduleService _dailyScheduleService;

    public DailySchedulesController(IDailyScheduleService dailyScheduleService)
    {
        _dailyScheduleService = dailyScheduleService;
    }

    /// <summary>
    /// 儲存每日時刻表
    /// </summary>
    /// <remarks>
    /// 用來新增或修改特定日期的時刻表。
    /// 
    /// - 第一次儲存：自動建立 DailySchedule（Draft 狀態）
    /// - 後續儲存：更新場次資料（全部替換）
    /// - 已販售的時刻表無法修改
    /// - 可傳入空陣列清空該日期所有場次
    /// 
    /// **範例請求**：
    /// ```json
    /// {
    ///   "showtimes": [
    ///     { "movieId": 1, "theaterId": 1, "startTime": "09:45" },
    ///     { "movieId": 2, "theaterId": 2, "startTime": "14:00" }
    ///   ]
    /// }
    /// ```
    /// 
    /// **驗證規則**：
    /// - 電影必須存在且放映日期在上映期間內
    /// - 影廳必須存在
    /// - 開始時間必須是 15 分鐘的倍數（如 09:00, 09:15, 09:30, 09:45）
    /// - 同一影廳的場次時間不能衝突
    /// </remarks>
    /// <param name="date">時刻表日期，格式：YYYY-MM-DD</param>
    /// <param name="request">時刻表資料</param>
    /// <response code="200">儲存成功</response>
    /// <response code="400">參數錯誤（電影/影廳不存在、時間格式錯誤等）</response>
    /// <response code="401">未授權</response>
    /// <response code="403">該日期已開始販售，無法修改</response>
    /// <response code="409">場次時間衝突</response>
    [HttpPut("{date}")]
    [ProducesResponseType(typeof(DailyScheduleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveDailySchedule(
        [FromRoute] string date,
        [FromBody] SaveDailyScheduleRequestDto request)
    {
        try
        {
            // 解析日期
            if (!DateTime.TryParse(date, out var scheduleDate))
            {
                return BadRequest(new { message = "日期格式錯誤，必須為 YYYY-MM-DD" });
            }

            var result = await _dailyScheduleService.SaveDailyScheduleAsync(scheduleDate, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // 根據錯誤訊息判斷回傳 403 或 409
            if (ex.Message.Contains("已開始販售"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            return Conflict(new { message = ex.Message });
        }
    }
}
