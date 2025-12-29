using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace betterthanvieshow.Controllers;

/// <summary>
/// 訂單控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Booking - 訂票流程")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// /api/orders 創建訂單
    /// </summary>
    /// <remarks>
    /// 此端點用於訂票流程的第四步：創建訂單。
    /// 使用者選擇座位後，點擊「確認訂單」按鈕創建訂單。系統將鎖定座位並開始 5 分鐘倒計時。
    /// 
    /// **前置條件**：
    /// - 場次必須存在
    /// - 場次日期的時刻表狀態必須為 OnSale
    /// - 座位必須存在且未被訂購
    /// - 座位數量必須在 1-6 範圍內
    /// 
    /// **即時更新**：
    /// - 訂單建立成功後，系統會透過 SignalR 廣播 `SeatStatusChanged` 事件給同場次的所有使用者，將狀態更新為 `sold`。
    /// 
    /// **請求範例 (JSON)**：
    /// ```json
    /// {
    ///   "showTimeId": 7,
    ///   "seatIds": [1, 2, 3]
    /// }
    /// ```
    /// 
    /// **成功回應範例 (201 Created)**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "訂單創建成功",
    ///   "data": {
    ///     "orderId": 1,
    ///     "orderNumber": "#ABC-12345",
    ///     "totalPrice": 900,
    ///     "expiresAt": "2025-12-28T10:05:00Z",
    ///     "ticketCount": 3,
    ///     "seats": [
    ///       {
    ///         "seatId": 1,
    ///         "rowName": "H",
    ///         "columnNumber": 12,
    ///         "ticketNumber": "12345678"
    ///       },
    ///       {
    ///         "seatId": 2,
    ///         "rowName": "H",
    ///         "columnNumber": 13,
    ///         "ticketNumber": "23456789"
    ///       },
    ///       {
    ///         "seatId": 3,
    ///         "rowName": "H",
    ///         "columnNumber": 14,
    ///         "ticketNumber": "34567890"
    ///       }
    ///     ]
    ///   }
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (400 Bad Request - 時刻表未開放販售)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "場次日期  2025-12-25 的時刻表尚未開放販售",
    ///   "data": null
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (404 Not Found - 場次不存在)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "場次 ID 999 不存在",
    ///   "data": null
    /// }
    /// ```
    /// 
    /// **失敗回應範例 (409 Conflict - 座位已被訂購)**：
    /// ```json
    /// {
    ///   "success": false,
    ///   "message": "座位 H12 已被訂購",
    ///   "data": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">創建訂單請求</param>
    /// <response code="201">訂單創建成功，返回訂單資訊</response>
    /// <response code="400">請求驗證失敗或業務規則錯誤（如時刻表未開放、座位數量超過限制）</response>
    /// <response code="401">未登入</response>
    /// <response code="404">場次或座位不存在</response>
    /// <response code="409">座位已被訂購</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CreateOrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
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

            return BadRequest(ApiResponse<CreateOrderResponseDto>.FailureResponse(
                "驗證失敗",
                errors
            ));
        }

        try
        {
            // 從 JWT Token 取得使用者 ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<CreateOrderResponseDto>.FailureResponse(
                    "無效的使用者憑證"
                ));
            }

            // 呼叫服務層創建訂單
            var result = await _orderService.CreateOrderAsync(userId, request);

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<CreateOrderResponseDto>.SuccessResponse(
                    result,
                    "訂單創建成功"
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            // 業務邏輯錯誤（場次不存在、座位已被訂購等）
            _logger.LogWarning(ex, "訂單創建失敗：{Message}", ex.Message);

            // 根據錯誤訊息返回適當的 HTTP 狀態碼
            if (ex.Message.Contains("不存在"))
            {
                return NotFound(ApiResponse<CreateOrderResponseDto>.FailureResponse(ex.Message));
            }
            else if (ex.Message.Contains("已被訂購"))
            {
                return Conflict(ApiResponse<CreateOrderResponseDto>.FailureResponse(ex.Message));
            }
            else
            {
                return BadRequest(ApiResponse<CreateOrderResponseDto>.FailureResponse(ex.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "創建訂單時發生未預期的錯誤");
            return StatusCode(500, ApiResponse<CreateOrderResponseDto>.FailureResponse(
                "伺服器錯誤，請稍後再試"
            ));
        }
    }
}
