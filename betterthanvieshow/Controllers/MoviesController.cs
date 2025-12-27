using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 電影管理控制器
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(
        IMovieService movieService,
        ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    /// <summary>
    /// 取得首頁電影資料
    /// </summary>
    /// <remarks>
    /// 此端點提供前台首頁所需的所有電影資料，包含：
    /// - **輪播圖電影**：標記為可輪播的電影（CanCarousel = true）
    /// - **本週前10**：目前為最新建立的10部正在上映電影，未來將改為根據票券銷售數量排序
    /// - **即將上映**：上映日期在今天之後的電影
    /// - **隨機推薦**：隨機推薦10部正在上映的電影
    /// - **所有電影**：正在上映 + 即將上映的完整列表
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// </remarks>
    /// <response code="200">成功取得首頁電影資料</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>首頁電影資料</returns>
    [HttpGet("~/api/movies/homepage")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<HomepageMoviesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HomepageMoviesResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHomepageMovies()
    {
        var result = await _movieService.GetHomepageMoviesAsync();

        if (!result.Success)
        {
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 搜尋電影
    /// </summary>
    /// <remarks>
    /// 根據關鍵字搜尋電影標題（Title）。
    /// 
    /// 只返回正在上映或即將上映的電影。
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// </remarks>
    /// <param name="keyword">搜尋關鍵字（必填，至少 1 個字元）</param>
    /// <response code="200">成功取得搜尋結果</response>
    /// <response code="400">關鍵字為空或無效</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>符合條件的電影列表</returns>
    [HttpGet("~/api/movies/search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<MovieSimpleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<MovieSimpleDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<List<MovieSimpleDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchMovies([FromQuery] string keyword)
    {
        // 驗證關鍵字
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<List<MovieSimpleDto>>.FailureResponse(
                "搜尋關鍵字不可為空"
            ));
        }

        var result = await _movieService.SearchMoviesAsync(keyword);

        if (!result.Success)
        {
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 取得電影詳情（前台）
    /// </summary>
    /// <remarks>
    /// 取得單一電影的完整資訊，包括：
    /// - 基本資訊：標題、分級、時長、類型
    /// - 詳細資訊：劇情介紹、導演、演員
    /// - 媒體：海報、預告片連結
    /// - 上映時間：上映日期、下映日期
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// </remarks>
    /// <param name="id">電影 ID</param>
    /// <response code="200">成功取得電影詳情</response>
    /// <response code="404">找不到指定的電影</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>電影詳情</returns>
    [HttpGet("~/api/movies/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMovieDetailForFrontend(int id)
    {
        var result = await _movieService.GetMovieByIdAsync(id);

        if (!result.Success)
        {
            if (result.Message?.Contains("找不到") == true)
            {
                return NotFound(result);
            }
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 取得電影的可訂票日期
    /// </summary>
    /// <remarks>
    /// 此端點用於訂票流程的第一步：選擇日期。
    /// 
    /// 返回該電影的完整資訊以及有場次且時刻表狀態為 **OnSale**（販售中）的日期列表。
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// 
    /// **業務規則**：
    /// - 只返回 `DailySchedule.Status = "OnSale"` 的日期
    /// - 草稿狀態 (`Draft`) 的場次不會出現在列表中
    /// - 日期按升序排序
    /// 
    /// **回應資料包含**：
    /// - 電影基本資訊（標題、分級、時長、類型）
    /// - 媒體資訊（海報、預告片）
    /// - 可訂票日期列表（含星期幾）
    /// </remarks>
    /// <param name="id">電影 ID</param>
    /// <response code="200">成功取得可訂票日期列表</response>
    /// <response code="404">找不到指定的電影</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>可訂票日期列表</returns>
    [HttpGet("~/api/movies/{id}/available-dates")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<MovieAvailableDatesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailableDates(int id)
    {
        try
        {
            var result = await _movieService.GetAvailableDatesAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"找不到 ID 為 {id} 的電影",
                    Data = null
                });
            }

            return Ok(new ApiResponse<MovieAvailableDatesResponseDto>
            {
                Success = true,
                Message = "成功取得可訂票日期",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得電影 {MovieId} 的可訂票日期時發生錯誤", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "取得可訂票日期時發生錯誤",
                Data = null
            });
        }
    }

    /// <summary>
    /// 取得電影在特定日期的場次列表
    /// </summary>
    /// <remarks>
    /// 此端點用於訂票流程的第二步：選擇場次。
    /// 
    /// 返回該電影在指定日期的所有場次資訊，包含影廳、時間、票價、可用座位數等。
    /// 
    /// **無需授權**，任何使用者皆可存取。
    /// 
    /// **業務規則**：
    /// - 只返回 `DailySchedule.Status = "OnSale"` 的場次
    /// - 草稿狀態 (`Draft`) 的場次不會出現在列表中
    /// - 場次按開始時間升序排序
    /// - 結束時間由系統動態計算（開始時間 + 電影時長）
    /// - 可用座位數 = 總座位數 - 已售出票券數（待支付、未使用、已使用狀態）
    /// - 票價根據影廳類型決定（一般數位 300元、4DX 380元、IMAX 380元）
    /// 
    /// **回應資料包含**：
    /// - 電影基本資訊（ID、名稱）
    /// - 查詢日期
    /// - 場次列表（影廳、時間、票價、座位資訊）
    /// </remarks>
    /// <param name="id">電影 ID</param>
    /// <param name="date">日期（格式：YYYY-MM-DD）</param>
    /// <response code="200">成功取得場次列表</response>
    /// <response code="400">日期格式無效</response>
    /// <response code="404">找不到指定的電影</response>
    /// <response code="500">伺服器內部錯誤</response>
    /// <returns>場次列表</returns>
    [HttpGet("~/api/movies/{id}/showtimes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<MovieShowtimesResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetShowtimesByDate(int id, [FromQuery] string date)
    {
        try
        {
            // 驗證日期格式
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, 
                System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "日期格式無效，請使用 YYYY-MM-DD 格式",
                    Data = null
                });
            }

            var result = await _movieService.GetShowtimesByDateAsync(id, parsedDate);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"找不到 ID 為 {id} 的電影",
                    Data = null
                });
            }

            return Ok(new ApiResponse<MovieShowtimesResponseDto>
            {
                Success = true,
                Message = "成功取得場次列表",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得電影 {MovieId} 在 {Date} 的場次列表時發生錯誤", id, date);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "取得場次列表時發生錯誤",
                Data = null
            });
        }
    }

    /// <summary>
    /// 取得所有電影
    /// </summary>
    /// <returns>電影列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<MovieListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<List<MovieListItemDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMovies()
    {
        var result = await _movieService.GetAllMoviesAsync();

        if (!result.Success)
        {
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 取得單一電影詳情
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <returns>電影詳情</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMovieById(int id)
    {
        var result = await _movieService.GetMovieByIdAsync(id);

        if (!result.Success)
        {
            if (result.Message?.Contains("找不到") == true)
            {
                return NotFound(result);
            }
            return StatusCode(500, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// 建立新電影
    /// </summary>
    /// <param name="request">建立電影請求</param>
    /// <returns>建立結果</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequestDto request)
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

            return BadRequest(ApiResponse<MovieResponseDto>.FailureResponse(
                "驗證失敗",
                errors
            ));
        }

        var result = await _movieService.CreateMovieAsync(request);

        if (!result.Success)
        {
            // 如果是業務邏輯驗證錯誤，回傳 400 Bad Request
            if (result.Message?.Contains("日期") == true)
            {
                return BadRequest(result);
            }

            // 其他錯誤回傳 500
            return StatusCode(500, result);
        }

        return CreatedAtAction(
            nameof(CreateMovie),
            new { id = result.Data?.Id },
            result
        );
    }

    /// <summary>
    /// 更新電影
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <param name="request">更新電影請求</param>
    /// <returns>更新結果</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMovie(int id, [FromBody] UpdateMovieRequestDto request)
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

            return BadRequest(ApiResponse<MovieResponseDto>.FailureResponse(
                "驗證失敗",
                errors
            ));
        }

        var result = await _movieService.UpdateMovieAsync(id, request);

        if (!result.Success)
        {
            // 如果是找不到電影，回傳 404
            if (result.Message?.Contains("找不到") == true)
            {
                return NotFound(result);
            }

            // 如果是業務邏輯驗證錯誤，回傳 400 Bad Request
            if (result.Message?.Contains("日期") == true)
            {
                return BadRequest(result);
            }

            // 其他錯誤回傳 500
            return StatusCode(500, result);
        }

        return Ok(result);
    }
}
