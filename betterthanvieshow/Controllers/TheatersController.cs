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
    /// <remarks>
    /// 建立一個新影廳，包含名稱、類型、樓層和座位配置。
    /// 
    /// **座位配置說明**：
    /// - 座位陣列為二維陣列，外層代表排，內層代表列
    /// - 座位類型：`Standard`（一般座位）、`Wheelchair`（無障礙座位）、`Aisle`（走道）、`Empty`（空位）
    /// 
    /// **請求範例 (JSON)**：
    /// ```json
    /// {
    ///   "name": "IMAX廳",
    ///   "type": "IMAX",
    ///   "floor": 5,
    ///   "rowCount": 3,
    ///   "columnCount": 5,
    ///   "seats": [
    ///     ["Standard", "Standard", "Aisle", "Standard", "Standard"],
    ///     ["Standard", "Standard", "Aisle", "Standard", "Standard"],
    ///     ["Wheelchair", "Standard", "Aisle", "Standard", "Wheelchair"]
    ///   ]
    /// }
    /// ```
    /// 
    /// **成功回應範例 (201 Created)**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "影廳建立成功",
    ///   "data": {
    ///     "id": 1,
    ///     "name": "IMAX廳",
    ///     "type": "IMAX",
    ///     "floor": 5,
    ///     "rowCount": 3,
    ///     "columnCount": 5,
    ///     "totalSeats": 12
    ///   }
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (400 Bad Request - 座位陣列不符)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "座位陣列大小與 RowCount 和 ColumnCount 不符",
    ///   "data": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">影廳資訊</param>
    /// <response code="201">影廳建立成功</response>
    /// <response code="400">欄位驗證失敗或座位配置錯誤</response>
    /// <response code="401">未授權（需登入）</response>
    /// <response code="403">權限不足（需 Admin 角色）</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TheaterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
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
