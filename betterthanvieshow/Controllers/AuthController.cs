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
[Tags("Auth - 會員驗證")]
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
    /// /api/auth/register 會員註冊
    /// </summary>
    /// <remarks>
    /// 使用使用者名稱、Email 和密碼進行註冊。
    /// 
    /// **請求範例 (JSON)**：
    /// ```json
    /// {
    ///   "name": "王小明",
    ///   "email": "wang@example.com",
    ///   "password": "Password123!"
    /// }
    /// ```
    /// 
    /// **成功回應範例 (200 OK)**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "註冊成功",
    ///   "data": {
    ///     "userId": 1,
    ///     "name": "王小明",
    ///     "email": "wang@example.com",
    ///     "role": "User",
    ///     "token": "eyJhbGciOiJIUzI1NiIs...",
    ///     "createdAt": "2023-10-25T10:00:00"
    ///   }
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (400 Bad Request - 驗證失敗)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "驗證失敗",
    ///   "data": {
    ///     "Email": [ "信箱格式不正確" ],
    ///     "Password": [ "密碼長度需大於 6 碼" ]
    ///   }
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (409 Conflict - 信箱已存在)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "該電子信箱已被註冊",
    ///   "data": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">註冊資訊</param>
    /// <response code="200">註冊成功，返回使用者資訊與 Token</response>
    /// <response code="400">欄位驗證失敗（如：密碼太短、Email 格式錯誤）</response>
    /// <response code="409">Email 已經被註冊</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// /api/auth/login 會員登入
    /// </summary>
    /// <remarks>
    /// 使用 Email 和密碼進行登入，獲取 JWT Token。
    /// 
    /// **請求範例 (JSON)**：
    /// ```json
    /// {
    ///   "email": "wang@example.com",
    ///   "password": "Password123!"
    /// }
    /// ```
    /// 
    /// **成功回應範例 (200 OK)**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "登入成功",
    ///   "data": {
    ///     "userId": 1,
    ///     "name": "王小明",
    ///     "email": "wang@example.com",
    ///     "role": "User",
    ///     "token": "eyJhbGciOiJIUzI1NiIs..."
    ///   }
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (401 Unauthorized - 登入失敗)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "帳號或密碼錯誤",
    ///   "data": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">登入資訊</param>
    /// <response code="200">登入成功，返回 Token</response>
    /// <response code="400">欄位格式錯誤</response>
    /// <response code="401">登入失敗（帳號或密碼錯誤）</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
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

            return BadRequest(ApiResponse<LoginResponseDto>.FailureResponse(
                "驗證失敗",
                errors
            ));
        }

        try
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登入過程發生錯誤");
            return StatusCode(500, ApiResponse<LoginResponseDto>.FailureResponse(
                "伺服器錯誤，請稍後再試"
            ));
        }
    }
}
