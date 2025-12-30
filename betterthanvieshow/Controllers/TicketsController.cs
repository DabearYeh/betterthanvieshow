using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 票券控制器
/// </summary>
[ApiController]
[Route("api/admin/tickets")]
[Authorize(Roles = "Admin")]
[Tags("Admin - 驗票管理")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        ITicketService ticketService,
        ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/admin/tickets/scan 掃描票券 QR Code
    /// </summary>
    /// <remarks>
    /// 此端點用於掃描票券 QR Code 並取得票券詳細資訊。
    /// 
    /// **功能說明**：
    /// - 管理者掃描顧客出示的票券 QR Code
    /// - 系統根據 QR Code 內容（票券編號）查詢票券資訊
    /// - 返回票券詳細資訊，包含場次、座位、影廳等資料
    /// - 此 API 僅查詢資訊，不修改票券狀態
    /// 
    /// **業務規則**：
    /// - QR Code 內容為票券編號（TicketNumber）
    /// - 票券不存在時返回 404
    /// - 可查詢任何狀態的票券（Pending, Unused, Used, Expired）
    /// - 僅限管理者使用
    /// 
    /// **範例**：
    /// ```
    /// GET /api/admin/tickets/scan?qrCode=TKT-12345678
    /// ```
    /// 
    /// **回應範例**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "成功取得票券資訊",
    ///   "data": {
    ///     "ticketId": 1,
    ///     "ticketNumber": "TKT-12345678",
    ///     "status": "Unused",
    ///     "movieTitle": "蝙蝠俠：黑暗騎士",
    ///     "showDate": "2025-12-31",
    ///     "showTime": "14:30",
    ///     "seatRow": "D",
    ///     "seatColumn": 12,
    ///     "seatLabel": "D 排 12 號",
    ///     "theaterName": "2A",
    ///     "theaterType": "Digital"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="qrCode">QR Code 內容（票券編號）</param>
    /// <returns>票券詳細資訊</returns>
    /// <response code="200">成功取得票券資訊</response>
    /// <response code="400">無效的 QR Code 格式</response>
    /// <response code="401">未登入</response>
    /// <response code="403">非管理者角色</response>
    /// <response code="404">票券不存在</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpGet("scan")]
    [ProducesResponseType(typeof(ApiResponse<TicketScanResponseDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ScanTicket([FromQuery] string qrCode)
    {
        try
        {
            // 驗證參數
            if (string.IsNullOrWhiteSpace(qrCode))
            {
                return BadRequest(new { message = "QR Code 不可為空" });
            }

            // 掃描票券
            var ticketInfo = await _ticketService.ScanTicketByQrCodeAsync(qrCode);

            return Ok(ApiResponse<TicketScanResponseDto>.SuccessResponse(
                ticketInfo,
                "成功取得票券資訊"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "掃描票券時發生錯誤: {QrCode}", qrCode);
            return StatusCode(500, new { message = $"系統錯誤: {ex.Message}" });
        }
    }
}
