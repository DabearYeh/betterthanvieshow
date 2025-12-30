# 訂單票卷詳情 API (Order Ticket Details) 實作計畫

## 目標
增強現有的 `GET /api/orders/{id}` API，使其回傳更詳細的票卷資訊，包含 QR Code 內容與票卷狀態，以支援前端「票卷細節」頁面的顯示。

## 變更項目

### 1. DTO (資料傳輸物件)
- **[NEW] TicketDetailDto**: 
    - 用於取代原本簡易的 `SeatInfoDto`。
    - 欄位包含：`TicketId` (票卷ID), `TicketNumber` (票卷編號), `SeatId` (座位ID), `RowName` (排), `ColumnNumber` (號), `Status` (狀態), `QrCodeContent` (QR Code 內容)。
- **[MODIFY] OrderDetailResponseDto**: 
    - 將 `Seats` 屬性的型別從 `List<SeatInfoDto>` 更改為 `List<TicketDetailDto>`。

### 2. Service 層 (OrderService)
- **GetOrderDetailAsync**:
    - 更新映射邏輯，改為建立 `TicketDetailDto` 物件。
    - 實作 QR Code 內容生成邏輯 (JSON 格式包含 ticketNumber, showTimeId, seatId)。
    - 從 `Ticket` 實體映射狀態與其他資訊。

### 3. Controller 層 (OrdersController)
- 無需修改程式碼，因回傳型別自動更新。

## 驗證結果
- 建立並執行了 HTTP 測試 `test-order-detail-v2.http`。
- 確認 API 回傳的 JSON 中，`seats` 陣列包含正確的 `qrCodeContent` 與  `status` 欄位。
