# 訂單票卷詳情 API (Walkthrough)

本文件紀錄如何測試與驗證「訂單票卷詳情 (含 QR Code)」功能。

## 功能概述
此功能擴充了原本的訂單詳情 API，在回傳的座位資訊中加入了票卷的詳細狀態與 QR Code 生成所需的內容字串。

- **API 路徑**: `GET /api/orders/{id}`
- **權限**: 需要登入 (Bearer Token)，且只能查詢自己的訂單。

## 測試步驟

### 1. 準備測試工具
我們使用 `.http` 檔案進行測試。

### 2. 測試腳本位置
- 專案路徑: `docs/tests/訂票API/test-order-detail-v2.http`
- 備份路徑: 本資料夾下的 `test-order-detail.http`

### 3. 執行測試
請依照以下順序執行請求：

1.  **登入 (Login)**:
    - 執行 `POST /api/auth/login` 取得 Access Token。

2.  **建立訂單 (Create Order)**:
    - 執行 `POST /api/orders` 建立一筆新訂單 (需注意選擇未被佔用的座位 ID)。
    - 記下回傳的 `orderId`。

3.  **查詢訂單詳情 (Get Order Detail)**:
    - 執行 `GET /api/orders/{orderId}`。
    - **預期結果**: 回傳 `200 OK`。
    - **驗證點**:
        - 檢查 `data.seats` 陣列。
        - 每個座位物件應包含 `qrCodeContent` 欄位 (非空字串)。
        - 每個座位物件應包含 `status` 欄位 (如 "Pending")。

### 4. 範例回應 (部分)
```json
"seats": [
    {
        "ticketId": 54,
        "ticketNumber": "79609841",
        "seatId": 8,
        "status": "Pending",
        "qrCodeContent": "{\"ticketNumber\":\"79609841\",\"showTimeId\":7,\"seatId\":8}"
    }
]
```
