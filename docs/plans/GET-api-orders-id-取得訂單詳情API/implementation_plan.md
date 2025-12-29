# 實作計畫：取得訂單詳情 API (GET /api/orders/{id})

## 1. 概述

實作訂單詳情查詢 API，用於「訂單確認與支付選擇」頁面。前端在使用者完成訂位後會跳轉至此頁面，顯示完整的訂單資訊、倒數計時、以及支付方式選擇。

- **URL**: `GET /api/orders/{id}`
- **權限**: 僅限登入會員 (Authorize)
- **安全性**: 使用者只能查詢自己的訂單

## 2. 前端頁面需求分析

根據設計圖，頁面需要顯示以下資訊：

### 電影資訊
- 海報圖片
- 片名（例：黑豹）
- 分級（例：普通級）
- 片長（例：2小時15分）

### 場次資訊
- 日期（例：12/15 三）
- 時間（例：下午 4:30）

### 影廳與座位
- 影廳名稱（例：鳳廳）
- 影廳類型（例：一般數位）
- 座位列表（例：H12, H13, H14）

### 倒數計時
- 過期時間戳記 (`ExpiresAt`)，由前端計算剩餘秒數

### 金額明細
- 應付總額（即票價總計，無額外手續費）

## 3. API 規格

### 請求 (Request)
- **Method**: GET
- **Header**: `Authorization: Bearer {token}`
- **Path Parameter**: `id` (訂單 ID)

### 回應 (Response)

#### 成功 (200 OK)
```json
{
  "success": true,
  "message": "成功取得訂單詳情",
  "data": {
    "orderId": 12,
    "orderNumber": "#LOG-12152",
    "status": "Pending",
    "expiresAt": "2025-12-29T10:38:11Z",
    "movie": {
      "title": "黑豹",
      "rating": "普通級",
      "duration": 135,
      "posterUrl": "https://..."
    },
    "showtime": {
      "date": "2025-12-15",
      "startTime": "16:30",
      "dayOfWeek": "三"
    },
    "theater": {
      "name": "鳳廳",
      "type": "一般數位"
    },
    "seats": [
      {
        "seatId": 1,
        "rowName": "H",
        "columnNumber": 12,
        "ticketNumber": "12345678"
      },
      {
        "seatId": 2,
        "rowName": "H",
        "columnNumber": 13,
        "ticketNumber": "23456789"
      }
    ],
    "totalAmount": 1050
  }
}
```

#### 失敗
- **404 Not Found**: 訂單不存在
- **403 Forbidden**: 無權查看此訂單（不是自己的訂單）
- **401 Unauthorized**: 未登入

## 4. DTO 設計

### OrderDetailResponseDto
完整的訂單詳情回應，包含所有嵌套資訊。

### 內嵌 DTO
- `OrderMovieInfoDto`: 電影資訊
- `OrderShowtimeInfoDto`: 場次資訊
- `OrderTheaterInfoDto`: 影廳資訊
- `OrderSeatInfoDto`: 座位與票券資訊（可重用現有的 `SeatInfoDto`）

**注意**：不需要 `OrderPricingDto`，直接在主 DTO 回傳 `TotalAmount` 即可（等同於訂單的 `TotalPrice`，無額外手續費）。

## 5. Repository Layer

### IOrderRepository
新增方法：
```csharp
Task<Order?> GetByIdWithFullDetailsAsync(int orderId);
```

**查詢邏輯**：
- Include `ShowTime` (場次)
  - ThenInclude `Movie` (電影)
  - ThenInclude `Theater` (影廳)
- Include `Tickets` (票券)
  - ThenInclude `Seat` (座位)

## 6. Service Layer

### IOrderService
新增方法：
```csharp
Task<OrderDetailResponseDto?> GetOrderDetailAsync(int orderId, int userId);
```

**業務邏輯**：
1. 呼叫 Repository 取得完整訂單資料
2. 驗證訂單是否屬於該使用者 (`order.UserId == userId`)
3. 組裝 DTO 並回傳（`TotalAmount` 直接使用訂單的 `TotalPrice`）

## 7. Controller Layer

### OrdersController
新增端點：
```csharp
[HttpGet("{id}")]
[Authorize]
public async Task<IActionResult> GetOrderDetail(int id)
```

**實作重點**：
- 從 JWT Token 解析 `userId`
- 呼叫 Service 層取得訂單詳情
- 錯誤處理：
  - 訂單不存在 → 404
  - 無權查看 → 403
  - 其他例外 → 500

## 8. 安全性考量

### 權限驗證
- 使用者只能查詢自己的訂單
- 在 Service 層明確檢查 `order.UserId == userId`
- 避免透過 URL 枚舉查看他人訂單

### 敏感資訊
- 不回傳使用者的完整個資（僅回傳訂單相關資訊）
- 如有第三方支付交易序號，不應在此 API 回傳

## 9. 測試計畫

### 功能測試
1. **正常查詢**：使用者查詢自己的訂單，回傳完整資訊
2. **訂單不存在**：查詢不存在的訂單 ID，回傳 404
3. **權限檢查**：嘗試查詢他人訂單，回傳 403
4. **未登入**：未帶 Token 查詢，回傳 401

### 資料完整性測試
- 驗證回傳的電影、場次、影廳、座位資訊正確無誤
- 驗證 `TotalAmount` 等於訂單的 `TotalPrice`（無額外費用）

## 10. 後續擴充

1. **訂單歷史**：額外提供 `GET /api/orders` 查詢使用者的所有訂單列表
2. **訂單狀態變更通知**：透過 SignalR 推播訂單狀態變更（如支付成功、自動取消）
