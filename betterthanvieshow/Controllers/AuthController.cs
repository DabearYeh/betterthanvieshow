using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 認證控制器（註冊、登入等）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Member")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 會員註冊
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <returns>註冊結果</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
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

            return BadRequest(ApiResponse<RegisterResponseDto>.FailureResponse(
                "驗證失敗",
                errors
            ));
        }

        try
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                // 信箱已存在的情況
                return Conflict(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "註冊過程發生錯誤");
            return StatusCode(500, ApiResponse<RegisterResponseDto>.FailureResponse(
                "伺服器錯誤，請稍後再試"
            ));
        }
    }
}
