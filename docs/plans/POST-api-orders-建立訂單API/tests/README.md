# 訂單 API 測試腳本說明

本資料夾包含用於驗證 `POST /api/orders` API 的 PowerShell 測試腳本。

## 檔案說明

- **`test_order_single.ps1`**: 
  - 基本的單一訂單建立測試。
  - 用於驗證最基礎的成功路徑。
  
- **`test_orders_full_scenarios.ps1`**: 
  - 全面的場景測試腳本。
  - 包含：座位衝突 (409)、場次不存在 (404)、座位不存在 (404)、訂票數量限制 (400) 以及正常訂位 (201)。
  - 測試結果會同時輸出至終端機與 `test_full_results.log`。

- **`temp_order.json`**: 
  - 測試用的請求 Payload。

## 如何執行測試

1. 確保伺服器已啟動 (`dotnet run`)。
2. 開啟 PowerShell 並切換至此資料夾（或專案根目錄）。
3. 執行指令：
   ```powershell
   powershell -ExecutionPolicy Bypass -File test_orders_full_scenarios.ps1
   ```

> [!NOTE]
> 測試腳本中使用的 JWT Token 有效期較短，若遇到 401 Unauthorized 錯誤，請重新登入並更新腳本中的 `$token` 變數。
