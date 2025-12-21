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
}
