# 實作計畫：建立訂單 API (POST /api/orders)

## 1. 概述
實作前台會員「建立訂單」功能。使用者在選擇場次與座位後，透過此 API 建立訂單，系統需驗證座位狀態、計算票價，並鎖定座位（透過產生 Ticket 紀錄）。狀態碼需嚴格使用英文 Enum 以符合資料庫 Check Constraint。

- **URL**: `POST /api/orders`
- **權限**: 僅限登入會員 (Authorize)

## 2. API 規格

### 請求 (Request)
- **Method**: POST
- **Header**: `Authorization: Bearer {token}`
- **Body**:
```json
{
  "showTimeId": 7,
  "seatIds": [1, 2]
}
```

### 回應 (Response)

#### 成功 (201 Created)
```json
{
  "success": true,
  "message": "訂單創建成功",
  "data": {
    "orderId": 12,
    "orderNumber": "#LOG-12152",
    "totalPrice": 760,
    "expiresAt": "2025-12-29T19:38:11Z",
    "ticketCount": 2,
    "seats": [
      {
        "seatId": 3,
        "rowName": "A",
        "columnNumber": 3,
        "ticketNumber": "12345678"
      },
      ...
    ]
  }
}
```

#### 失敗
- **400 Bad Request**: 驗證失敗 (如座位數量 < 1 或 > 6)。
- **401 Unauthorized**: 未登入。
- **404 Not Found**: 場次不存在或座位不存在。
- **409 Conflict**: 座位已被訂購。
- **500 Internal Server Error**: 伺服器內部錯誤。

## 3. 資料模型變更 (Important Refinement)

由於 SQL Server Check Constraint 對於中文字串的處理問題，所有狀態欄位必須統一使用**英文**。

### Order
- `Status`: `Pending`, `Paid`, `Cancelled` (Default: `Pending`)
- Table Check Constraint: `[Status] IN ('Pending', 'Paid', 'Cancelled')`

### Ticket
- `Status`: `Pending`, `Unused`, `Used`, `Expired` (Default: `Pending`)
- Table Check Constraint: `[Status] IN ('Pending', 'Unused', 'Used', 'Expired')`

## 4. 核心邏輯 (Service Layer)
`OrderService.CreateOrderAsync`:
1.  **驗證場次**: 檢查 `ShowTimeId` 是否存在。
2.  **驗證時刻表**: 檢查該場次日期的 `DailySchedule` 狀態是否為 `OnSale`。
3.  **驗證座位數量**: 限制 1-6 張。
4.  **驗證座位狀態**: 
    - 檢查 `SeatIds` 是否存在。
    - 檢查是否已被佔用 (`TicketRepository.IsSeatOccupiedAsync`)。
5.  **建立訂單**:
    - 生成唯一 `OrderNumber` (#ABC-12345)。
    - 計算總金額 (`CalculateTicketPrice` by TheaterType)。
    - 設定狀態為 `Pending`。
6.  **建立票券**:
    - 為每個座位建立 `Ticket`。
    - 生成唯一 `TicketNumber`。
    - 設定狀態為 `Pending`。
7.  **回傳 DTO**: 組裝 `CreateOrderResponseDto`.

## 5. 測試計畫
- [x] **單一訂單測試**: 使用 PowerShell 腳本 `test_order_single.ps1` 驗證成功建立訂單。
- [x] **錯誤處理測試**: 驗證座位衝突、無效場次等情況 (已於之前的 Conversation 完成)。
- [x] **資料庫約束測試**: 驗證英文狀態碼是否能正確寫入 DB。
