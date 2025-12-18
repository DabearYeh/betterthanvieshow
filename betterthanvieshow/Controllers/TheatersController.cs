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
}
