# 查看票卷 API (Walkthrough)

本文件紀錄如何測試與驗證「查看票卷 (My Orders)」功能。

## 功能概述
此功能允許使用者查看他們購買過的所有電影票卷訂單，包含未付款、已付款及已取消的紀錄。

- **API 路徑**: `GET /api/orders/mine`
- **權限**: 需要登入 (Bearer Token)

## 測試步驟

### 1. 準備測試工具
我們使用 `.http` 檔案進行測試，這是一種可以直接在 VS Code 或其他支援的 IDE 中執行的 HTTP 請求腳本。

### 2. 測試腳本位置
- 專案路徑: `docs/tests/訂票API/test-my-orders.http`
- 備份路徑: 本資料夾下的 `test-my-orders.http`

### 3. 執行測試
請依照以下順序執行請求：

1.  **註冊/登入 (Register/Login)**:
    - 執行 `POST /api/auth/register` 註冊一個測試帳號。
    - 執行 `POST /api/auth/login` 取得 Access Token (`customerToken`)。

2.  **查詢訂單 (Get My Orders) - 初始狀態**:
    - 執行 `GET /api/orders/mine`。
    - **預期結果**: 回傳 `200 OK`，且 `data` 為空陣列 `[]` (如果是新帳號)。

3.  **建立訂單 (Create Order)**:
    - 執行 `POST /api/orders` 建立一筆新訂單（需確保 payload 中的 seatIds 尚未被佔用）。
    - **預期結果**: 回傳 `201 Created`。

4.  **查詢訂單 (Get My Orders) - 有資料**:
    - 再次執行 `GET /api/orders/mine`。
    - **預期結果**: 回傳 `200 OK`，且 `data` 陣列中包含剛才建立的訂單資訊。
    - **驗證點**:
        - `movieTitle`: 是否正確顯示電影名稱。
        - `status`: 應顯示為 `Pending` 或 `Paid`。
        - `isUsed`: 若是未來的場次，應為 `false`。

### 4. 常見問題排查
- **401 Unauthorized**: 確認 Header 是否有帶入正確的 Bearer Token。
- **404 Not Found**: 確認 API 路徑是否正確 (是 `/mine` 不是 `/id`)，且伺服器已更新至最新版本。
