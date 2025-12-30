# 查看票卷 API (My Tickets) 實作計畫

## 目標
實作 `GET /api/orders/mine` API，允許使用者查詢自己的訂單歷史紀錄（我的票卷）。

## 實作細節

### 1. DTO (資料傳輸物件)
- **OrderHistoryResponseDto**: 定義回傳給前端的資料結構。
    - `OrderId`: 訂單 ID
    - `MovieTitle`: 電影名稱
    - `PosterUrl`: 海報 URL
    - `ShowTime`: 場次時間
    - `TicketCount`: 票數
    - `DurationMinutes`: 片長
    - `Status`: 訂單狀態 (Pending, Paid, Cancelled)
    - `IsUsed`: 是否已使用/過期 (由後端根據時間判斷)

### 2. Repository 層
- **IOrderRepository / OrderRepository**:
    - 新增 `GetByUserIdAsync(int userId)` 方法。
    - 使用 Entity Framework Core 的 `.Include()` 載入關聯資料 (ShowTime, Movie)。
    - 實作排序：依場次日期與時間倒序排列 (最新的在前)。

### 3. Service 層
- **IOrderService / OrderService**:
    - 新增 `GetMyOrdersAsync(int userId)` 方法。
    - 邏輯處理：
        - 呼叫 Repository 取得資料。
        - 計算 `ShowTime` (結合 Date 與 Time)。
        - 判斷 `IsUsed`：若 `(場次時間 + 片長) < 當前時間` 則視為已使用/已結束。
        - 轉換為 DTO 回傳。

### 4. Controller 層
- **OrdersController**:
    - 新增 `GET /api/orders/mine` 端點。
    - 設定 `[Authorize]` 確保僅登入使用者可存取。
    - 從使用者 Token 解析 `OrderId` 並呼叫 Service。

## 驗證結果
- 已建立 HTTP 測試腳本 `test-my-orders.http`。
- 測試通過：
    - 未登入 -> 401 Unauthorized
    - 登入 -> 200 OK (回傳 JSON 列表)
