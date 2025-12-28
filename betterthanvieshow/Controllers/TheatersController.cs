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
[Tags("Admin/Theaters - 影廳管理")]
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
    /// /api/admin/theaters 取得所有影廳
    /// </summary>
    /// <remarks>
    /// 取得系統內所有影廳的摘要清單。
    /// 
    /// **用途**：
    /// - 用於後台影廳管理列表。
    /// - 快速查看影廳類型、樓層與統計資訊。
    /// 
    /// **回傳資料包含**：
    /// - 影廳 ID、名稱、類型 (IMAX, 4DX, Standard)
    /// - 所在的樓層 (Floor)
    /// - 統計資訊：總座位數 (TotalSeats)
    /// </remarks>
    /// <response code="200">成功取得影廳列表</response>
    /// <response code="401">未授權（需登入）</response>
    /// <response code="403">權限不足（需 Admin 角色）</response>
    /// <response code="500">伺服器內部錯誤</response>
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
    /// /api/admin/theaters/{id} 根據 ID 取得影廳詳細資訊（含座位表）
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <remarks>
    /// 取得單一影廳的完整設定值，包含「二維座位矩陣」。
    /// 
    /// **用途**：
    /// - 影廳編輯頁面的資料預填。
    /// - 查看影廳的座位實際佈局情況。
    /// 
    /// **座位表結構 (Seats)**：
    /// - 回傳一個二維陣列 `string[][]`。
    /// - 外層代表「排」，內層代表「列」。
    /// - 座位標記：`Standard`、`Wheelchair`、`Aisle` (走道)、`Empty` (空位)。
    /// </remarks>
    /// <response code="200">成功取得影廳詳情與座位配置</response>
    /// <response code="401">未授權（需登入）</response>
    /// <response code="403">權限不足（需 Admin 角色）</response>
    /// <response code="404">找不到指定的影廳</response>
    /// <response code="500">伺服器內部錯誤</response>
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
    /// /api/admin/theaters 建立新影廳
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
    /// <response code="400">驗證失敗（如：欄位必填、座位矩陣大小不符、有無效的座位類型標記等）</response>
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
    /// /api/admin/theaters/{id} 刪除影廳
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <remarks>
    /// 從系統中移除指定的影廳。此操作具有連鎖限制。
    /// 
    /// **業務規則 (Business Rules)**：
    /// - **安全性限制**：若影廳已具備任何相關場次（Showtimes），系統將禁止刪除，並回傳 400 Bad Request。
    /// - **操作不可逆**：一旦刪除，影廳及其所有座位設定將永久移除。
    /// </remarks>
    /// <response code="200">影廳刪除成功</response>
    /// <response code="400">無法刪除（因影廳已有關聯場次或存在未完成訂單）</response>
    /// <response code="401">未授權（需登入）</response>
    /// <response code="403">權限不足（需 Admin 角色）</response>
    /// <response code="404">找不到指定的影廳</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>刪除結果</returns>
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
