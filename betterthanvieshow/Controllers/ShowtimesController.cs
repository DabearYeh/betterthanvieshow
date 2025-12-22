using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 場次管理 API
/// </summary>
[ApiController]
[Route("api/admin/showtimes")]
[Authorize(Roles = "Admin")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;

    public ShowtimesController(IShowtimeService showtimeService)
    {
        _showtimeService = showtimeService;
    }

    /// <summary>
    /// 新增場次
    /// </summary>
    /// <param name="dto">新增場次請求</param>
    /// <returns>建立的場次資訊</returns>
    /// <response code="201">場次建立成功</response>
    /// <response code="400">請求參數無效</response>
    /// <response code="401">未授權</response>
    /// <response code="403">該日期時刻表已開始販售，無法新增</response>
    /// <response code="409">時間衝突</response>
    [HttpPost]
    [ProducesResponseType(typeof(ShowtimeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeRequestDto dto)
    {
        try
        {
            var result = await _showtimeService.CreateShowtimeAsync(dto);
            return CreatedAtAction(nameof(CreateShowtime), new { id = result.Id }, result);
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
