using betterthanvieshow.Models.DTOs.Payment;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 付款相關 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Payments - 付款管理")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// POST /api/payments/line-pay/request 發起 LINE Pay 付款請求
    /// </summary>
    /// <param name="request">付款請求資訊</param>
    /// <returns>付款頁面網址與交易 ID</returns>
    /// <response code="200">成功取得付款網址</response>
    /// <response code="400">訂單狀態錯誤或已過期</response>
    /// <response code="401">未登入</response>
    /// <response code="403">無權訪問此訂單</response>
    /// <response code="404">訂單不存在</response>
    /// <response code="500">LINE Pay API 錯誤</response>
    [HttpPost("line-pay/request")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Payments - 付款管理")]
    public async Task<ActionResult<PaymentRequestResponseDto>> CreatePaymentRequest(
        [FromBody] PaymentRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _paymentService.CreatePaymentRequestAsync(request.OrderId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("不存在") 
                ? NotFound(new { message = ex.Message })
                : BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "發起付款請求失敗", detail = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/payments/line-pay/confirm 確認 LINE Pay 付款完成
    /// </summary>
    /// <param name="request">付款確認資訊（含交易 ID 與訂單 ID）</param>
    /// <returns>付款結果與票券資訊</returns>
    /// <response code="200">付款確認成功，訂單已更新為「已付款」</response>
    /// <response code="400">訂單狀態錯誤或 LINE Pay 確認失敗</response>
    /// <response code="401">未登入</response>
    /// <response code="403">無權訪問此訂單</response>
    /// <response code="404">訂單不存在</response>
    /// <response code="500">系統錯誤</response>
    [HttpPost("line-pay/confirm")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentConfirmResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Payments - 付款管理")]
    public async Task<ActionResult<PaymentConfirmResponseDto>> ConfirmPayment(
        [FromBody] PaymentConfirmRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _paymentService.ConfirmPaymentAsync(
                request.TransactionId, 
                request.OrderId, 
                userId
            );
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("不存在") 
                ? NotFound(new { message = ex.Message })
                : BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "確認付款失敗", detail = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/payments/line-pay/cancel 處理 LINE Pay 付款取消
    /// </summary>
    /// <param name="orderId">訂單 ID（從 query string 取得）</param>
    /// <returns>跳轉至前端取消頁面</returns>
    /// <response code="302">跳轉至前端取消頁面</response>
    [HttpGet("line-pay/cancel")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [Tags("Payments - 付款管理")]
    public IActionResult CancelPayment([FromQuery] int orderId)
    {
        // TODO: 可記錄取消事件到 Log
        // 跳轉回前端的取消頁面，帶上 orderId 參數
        var frontendCancelUrl = $"http://localhost:3000/checkout/cancel?orderId={orderId}";
        return Redirect(frontendCancelUrl);
    }
}
