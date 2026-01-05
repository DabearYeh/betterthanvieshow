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
[Tags("Orders - 訂單管理")]
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
    /// POST /api/orders 創建訂單
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

    /// <summary>
    /// GET /api/orders/{id} 取得訂單詳情
    /// </summary>
    /// <remarks>
    /// 此端點用於訂票流程的第五步：訂單確認與支付選擇頁面。
    /// 
    /// 使用者在完成訂位後將跳轉至此頁面，顯示完整訂單資訊、倒數計時與支付方式選擇。
    /// 
    /// **安全性**：
    /// - 需要 JWT Token 認證
    /// - 使用者只能查詢自己的訂單
    /// 
    /// **回應資料包含**：
    /// - 訂單基本資訊（訂單編號、狀態、過期時間）
    /// - 電影資訊（片名、分級、片長、海報）
    /// - 場次資訊（日期、時間、星期幾）
    /// - 影廳資訊（影廳名稱、類型）
    /// - 座位與票券列表（包含 QR Code）
    /// - 付款方式（如：Line Pay，未付款則為 null）
    /// - 應付總額
    /// 
    /// **成功回應範例 (200 OK)**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "成功取得訂單詳情",
    ///   "data": {
    ///     "orderId": 1,
    ///     "orderNumber": "#BKA-13005",
    ///     "status": "Paid",
    ///     "expiresAt": "2025-12-30T10:35:00Z",
    ///     "movie": {
    ///       "title": "黑豹",
    ///       "rating": "PG-13",
    ///       "duration": 135,
    ///       "posterUrl": "https://example.com/poster.jpg"
    ///     },
    ///     "showtime": {
    ///       "date": "2025-12-15",
    ///       "startTime": "16:30",
    ///       "dayOfWeek": "三"
    ///     },
    ///     "theater": {
    ///       "name": "鳳廳",
    ///       "type": "Digital"
    ///     },
    ///     "seats": [
    ///       {
    ///         "ticketId": 1,
    ///         "ticketNumber": "TKT-12345678",
    ///         "seatId": 1,
    ///         "rowName": "H",
    ///         "columnNumber": 12,
    ///         "status": "Unused",
    ///         "qrCodeContent": "VElDS0VUOlRLVC0xMjM0NTY3OA=="
    ///       }
    ///     ],
    ///     "paymentMethod": "Line Pay",
    ///     "totalAmount": 300
    ///   }
    /// }
    /// ```
    /// 
    /// **注意事項**：
    /// - `qrCodeContent` 為 Base64 編碼字串，前端可用於生成 QR Code 圖片
    /// - 只有已付款的訂單才會有 QR Code 內容
    /// </remarks>
    /// <param name="id">訂單 ID</param>
    /// <response code="200">成功取得訂單詳情</response>
    /// <response code="401">未登入</response>
    /// <response code="403">無權查看此訂單（不是自己的訂單）</response>
    /// <response code="404">訂單不存在</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderDetail(int id)
    {
        try
        {
            // 從 JWT Token 取得使用者 ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("無效的使用者身份"));
            }

            // 取得訂單詳情
            var orderDetail = await _orderService.GetOrderDetailAsync(id, userId);

            if (orderDetail == null)
            {
                // Service 回傳 null 可能是訂單不存在或無權查看
                // 為了安全性，統一回傳 404（避免透露訂單是否存在）
                return NotFound(ApiResponse<object>.FailureResponse("找不到指定的訂單"));
            }

            return Ok(ApiResponse<OrderDetailResponseDto>.SuccessResponse(
                orderDetail,
                "成功取得訂單詳情"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得訂單 {OrderId} 詳情時發生錯誤", id);
            return StatusCode(500, ApiResponse<object>.FailureResponse("伺服器錯誤，請稍後再試"));
        }
    }

    /// <summary>
    /// GET /api/orders 取得所有訂單
    /// </summary>
    /// <remarks>
    /// 取得當前使用者的所有「已付款」訂單。
    /// 
    /// **過濾條件**：只返回 Status 為 "Paid" 的訂單（已移除未付款和已取消的訂單）。
    /// 
    /// **排序**：按場次時間倒序排列（最新的場次在最前面）。
    /// 
    /// **IsUsed 判定**：
    /// - 若場次時間已過，`isUsed` 為 true。
    /// 
    /// **成功回應範例 (200 OK)**：
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "成功取得訂單列表",
    ///   "data": [
    ///     {
    ///       "orderId": 1,
    ///       "movieTitle": "黑豹",
    ///       "posterUrl": "https://example.com/poster.jpg",
    ///       "showTime": "2025-12-15T16:30:00",
    ///       "ticketCount": 3,
    ///       "durationMinutes": 135,
    ///       "status": "Paid",
    ///       "isUsed": true
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">成功取得訂單列表</response>
    /// <response code="401">未登入</response>
    /// <response code="500">伺服器內部錯誤</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<OrderHistoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("無效的使用者身份"));
            }

            var orders = await _orderService.GetMyOrdersAsync(userId);

            return Ok(ApiResponse<List<OrderHistoryResponseDto>>.SuccessResponse(
                orders,
                "成功取得訂單列表"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得使用者訂單列表時發生錯誤");
            return StatusCode(500, ApiResponse<object>.FailureResponse("伺服器錯誤，請稍後再試"));
        }
    }
}
