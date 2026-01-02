using System.Security.Claims;
using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 使用者控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("User - 會員管理")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/users/profile 取得個人資料
    /// </summary>
    /// <remarks>
    /// 取得目前登入使用者的個人資料（姓名、Email）。
    /// 需要 Bearer Token。
    /// </remarks>
    /// <response code="200">成功取得資料</response>
    /// <response code="401">未授權</response>
    /// <response code="404">找不到使用者</response>
    /// <response code="500">伺服器錯誤</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("無法識別使用者身分"));
            }

            var result = await _userService.GetUserProfileAsync(userId);

            if (!result.Success)
            {
                if (result.Message.Contains("找不到"))
                {
                    return NotFound(result);
                }
                return StatusCode(500, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得個人資料失敗");
            return StatusCode(500, ApiResponse<object>.FailureResponse("伺服器錯誤"));
        }
    }
}
