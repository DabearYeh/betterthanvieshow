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
