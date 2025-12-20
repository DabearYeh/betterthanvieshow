using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 影廳管理控制器
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class TheatersController : ControllerBase
{
    private readonly ITheaterService _theaterService;
    private readonly ILogger<TheatersController> _logger;

    public TheatersController(
        ITheaterService theaterService,
        ILogger<TheatersController> logger)
    {
        _theaterService = theaterService;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有影廳
    /// </summary>
    /// <returns>影廳列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TheaterResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<List<TheaterResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTheaters()
    {
        var result = await _theaterService.GetAllTheatersAsync();

        if (!result.Success)
        {
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 根據 ID 取得影廳詳細資訊（含座位表）
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>影廳詳細資訊</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TheaterDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TheaterDetailResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TheaterDetailResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTheaterById(int id)
    {
        var result = await _theaterService.GetTheaterByIdAsync(id);

        if (!result.Success)
        {
            // 如果是找不到影廳，回傳 404
            if (result.Message?.Contains("找不到") == true)
            {
                return NotFound(result);
            }
            
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 建立新影廳
    /// </summary>
    /// <param name="request">建立影廳請求</param>
    /// <returns>建立結果</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TheaterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TheaterResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TheaterResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTheater([FromBody] CreateTheaterRequestDto request)
    {
        // 檢查模型驗證
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(ApiResponse<TheaterResponseDto>.FailureResponse(
                "驗證失敗",
                errors
            ));
        }

        var result = await _theaterService.CreateTheaterAsync(request);

        if (!result.Success)
        {
            // 如果是業務邏輯驗證錯誤，回傳 400 Bad Request
            if (result.Message?.Contains("座位陣列") == true || 
                result.Message?.Contains("影廳必須") == true ||
                result.Message?.Contains("不符") == true)
            {
                return BadRequest(result);
            }
            
            // 其他錯誤回傳 500
            return StatusCode(500, result);
        }

        return CreatedAtAction(
            nameof(GetAllTheaters),
            new { id = result.Data?.Id },
            result
        );
    }

    /// <summary>
    /// 刪除影廳
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>刪除結果</returns>
    /// <remarks>
    /// 注意：影廳只有在沒有關聯場次時才能刪除
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTheater(int id)
    {
        var result = await _theaterService.DeleteTheaterAsync(id);

        if (!result.Success)
        {
            // 如果是找不到影廳，回傳 404
            if (result.Message?.Contains("找不到") == true)
            {
                return NotFound(result);
            }

            // 如果是有場次無法刪除，回傳 400
            if (result.Message?.Contains("場次") == true)
            {
                return BadRequest(result);
            }

            // 其他錯誤回傳 500
            return StatusCode(500, result);
        }

        return Ok(result);
    }
}
