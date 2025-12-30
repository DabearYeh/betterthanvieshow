# 訂單票卷詳情 API (Walkthrough) - v2 票卷細節

本文件紀錄如何測試與驗證 v2 版本的訂單詳情功能，重點在於驗證 **QR Code** 與 **票卷狀態** 的回傳。

## 測試環境
- **API Endpoint**: `GET /api/orders/{id}`
- **測試工具**: VS Code REST Client (使用 `tests/test-order-detail-v2.http`)

## 測試步驟

### 1. 準備測試腳本
使用隨附的 `tests/test-order-detail-v2.http` 檔案。

### 2. 執行驗證流程
1.  **取得 Token (Login)**
    - 呼叫 `POST /api/auth/login`。
    - 取得 token 並自動存入變數。

2.  **建立測試訂單 (Create Order)**
    - 呼叫 `POST /api/orders` 建立一筆新訂單。
    - **注意**: 若回應 "409 Conflict (座位已被訂購)"，請手動修改 payload 中的 `seatIds` (例如改為 `[10, 11]`) 並重試。
    - 成功後，系統會自動捕捉 `orderId`。

3.  **查詢詳情 (Get Detail)**
    - 呼叫 `GET /api/orders/{orderId}`。

### 3. 驗證結果
檢查 JSON 回應中的 `data.seats` 部分：

- **欄位完整性**: 每個座位物件必須包含 `ticketId`, `status`, `qrCodeContent`。
- **QR Code 內容**: `qrCodeContent` 應為一個 JSON 字串，包含該張票的 ticketNumber 與 seatId。
- **狀態**: 新建立的訂單，票卷狀態應為 `Pending`。

範例正確回應：
```json
"seats": [
  {
    "ticketId": 105,
    "ticketNumber": "12345678",
    "seatId": 10,
    "rowName": "B",
    "columnNumber": 5,
    "status": "Pending",
    "qrCodeContent": "{\"ticketNumber\":\"12345678\",\"showTimeId\":7,\"seatId\":10}"
  }
]
```
