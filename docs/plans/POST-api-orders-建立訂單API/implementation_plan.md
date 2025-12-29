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

## 6. 後續最佳化與進階功能 (Advanced Features)

### 1. WebSocket 座位狀態廣播 (SignalR)
**【現狀說明】**
目前當使用者 A 成功訂票（POST /api/orders）時，資料庫的 Ticket 狀態會變成 Pending。這代表座位被鎖定了。

**【問題點】**
如果此時使用者 B 同時也在看「同一個場次」的選擇座位頁面，使用者的畫面**不會自動更新**。使用者 B 看到的座位可能還是「可選」，直到他點擊預約時被系統擋下報錯（409 Conflict）。這會導致差勁的使用者體驗。

**【缺少的邏輯】**
我們需要在 `OrderService.CreateOrderAsync` 執行成功後，加入以下動作：
- **觸發廣播**：呼叫 SignalR 的 Hub。
- **通知內容**：告訴所有正在看該場次的用戶：「座位 X, Y 正式進入鎖定狀態，請更新畫面」。
- **預期效果**：使用者 B 的畫面會即時看到該座位變成不可選取的灰色。

### 2. 五分鐘倒計時背景處理 (Background Task)
**【現狀說明】**
我們在建立訂單時，已經在 Order 資料表設定了 ExpiresAt（例如 10:05 分過期）。

**【問題點】**
這只是一個「靜態的時間戳記」。如果使用者在 10:00 訂票，但隨即關掉瀏覽器跑去睡覺，而不進行付款，這張訂單會永遠停留在 Pending 狀態。
- **座位鎖死**：因為系統判斷 Pending 的座位是「已佔用」，所以這幾個好位置會被佔據，直到管理員手動刪除，這對影城來說是營收損失。

**【缺少的邏輯】**
我們需要一個「排程任務」或「背景服務」(Worker Service)：
- **運作方式**：每分鐘掃描一次資料庫。
- **搜尋條件**：找出所有 `Status == "Pending"` 且 `ExpiresAt < 現在時間` 的訂單。
- **處置方案**：將這些訂單狀態改為 "Cancelled"。
- **預期效果**：當背景任務把訂單取消後,那些因為這筆訂單產生的 Ticket（票券）也就失效了，座位便會**自動變回「可用 (Available)」**，讓其他客人可以訂購。

---

## 7. SignalR 與背景任務實作 (已完成)

### A. WebSocket 座位狀態廣播 (SignalR)
當使用者成功建立訂單後，系統將廣播該場次的座位狀態變更給所有連接中的客戶端。

#### 實作細節：
- **觸發位置**：`OrderService.CreateOrderAsync` 儲存訂單成功後。
- **廣播事件**：`SeatStatusChanged`。
- **傳輸內容**：
  ```json
  {
    "showtimeId": 7,
    "seatIds": [1, 2],
    "status": "sold"  // 在前台視為已鎖定/已售出
  }
  ```

#### 程式碼變更：
- **[OrderService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/OrderService.cs)**：注入 `IHubContext<ShowtimeHub>`，在訂單建立成功後廣播。

### B. 五分鐘倒計時背景處理 (Background Task)
系統將定時掃描資料庫中的過期訂單（Pending 狀態且超過 `ExpiresAt`），並將其自動取消。

#### 實作細節：
- **服務類別**：`ExpiredOrderCleanupService` 繼承自 `BackgroundService`。
- **執行頻率**：每分鐘執行一次。
- **處理邏輯**：
  1. 找出所有 `Status = "Pending"` 且 `ExpiresAt < DateTime.UtcNow` 的訂單。
  2. 將訂單狀態更新為 `"Cancelled"`。
  3. 將對應的票券狀態更新為 `"Expired"`。
  4. **廣播通知**：將釋放的座位透過 SignalR 廣播為 `"available"` 狀態。

#### 程式碼變更：
- **[ExpiredOrderCleanupService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Background/ExpiredOrderCleanupService.cs)** (新增)：背景服務實作。
- **[Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)**：註冊 `HostedService`。

### 驗證結果
- ✅ 背景服務成功啟動並自動清理 10 筆過期訂單
- ✅ SignalR 廣播已整合至訂單建立流程

