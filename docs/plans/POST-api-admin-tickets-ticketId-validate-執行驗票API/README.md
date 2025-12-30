# POST /api/admin/tickets/{ticketId}/validate - 執行驗票 API

## API 概述

此 API 用於管理者執行驗票動作，將票券狀態從 `Unused` 更新為 `Used`，並建立驗票記錄。

- **端點**: `POST /api/admin/tickets/{ticketId}/validate`
- **授權**: 需要 Admin 角色
- **功能**: 執行驗票並更新票券狀態

## 資料夾結構

```
POST-api-admin-tickets-ticketId-validate-執行驗票API/
├── README.md                    # 本檔案
├── implementation_plan.md       # 實作計劃
├── walkthrough.md              # 驗收報告
└── tests/                      # 測試相關
    └── test-validate-ticket.http   # HTTP 測試腳本
```

## 文檔說明

### 1. implementation_plan.md
實作計劃文件，包含：
- API 設計規格與業務規則
- 資料模型（TicketValidateLog 實體）
- Repository、Service、Controller 層設計
- 資料庫配置與遷移計劃
- 驗證計劃

### 2. walkthrough.md
驗收報告文件，包含：
- 已完成功能摘要
- 程式碼變更清單
- 測試結果與功能驗證
- API 回應範例

### 3. tests/test-validate-ticket.http
HTTP 測試腳本，包含：
- 驗票成功（Unused 票券）
- 重複驗票（已使用票券）
- 驗票已過期票券
- 驗票不存在的票券
- 授權測試

## 快速開始

### 前置準備

1. 確保應用程式正在運行：
   ```bash
   cd betterthanvieshow
   dotnet run
   ```

2. 取得 Admin Token：
   ```http
   POST http://localhost:5041/api/auth/login
   Content-Type: application/json

   {
     "email": "admin1234@gmail.com",
     "password": "Admin@123"
   }
   ```

### 測試 API

使用 HTTP 測試工具執行：

1. **先掃描票券，確認狀態為 Unused**：
   ```http
   GET http://localhost:5041/api/admin/tickets/scan?qrCode={票券編號}
   Authorization: Bearer {你的_admin_token}
   ```

2. **執行驗票**：
   ```http
   POST http://localhost:5041/api/admin/tickets/{ticketId}/validate
   Authorization: Bearer {你的_admin_token}
   ```

3. **再次掃描，確認狀態已變為 Used**

## API 回應範例

### 成功回應 (200 OK)
```json
{
  "message": "驗票成功"
}
```

### 錯誤回應 (400 Bad Request - 已使用)
```json
{
  "message": "票券已使用"
}
```

### 錯誤回應 (400 Bad Request - 已過期)
```json
{
  "message": "票券已過期"
}
```

### 錯誤回應 (404 Not Found)
```json
{
  "message": "票券不存在"
}
```

## 業務規則

驗票 API 會根據票券狀態決定是否允許驗票：

- **Unused** → ✅ 允許驗票 → 更新為 `Used`
- **Used** → ❌ 拒絕驗票 → 回傳「票券已使用」
- **Expired** → ❌ 拒絕驗票 → 回傳「票券已過期」
- **Pending** → ❌ 拒絕驗票 → 回傳「票券未支付」
- **不存在** → ❌ 拒絕驗票 → 回傳「票券不存在」

### 驗票記錄

每次驗票嘗試（無論成功或失敗）都會建立 `TicketValidateLog` 記錄：

- **ValidationResult = true**：驗票成功
- **ValidationResult = false**：驗票失敗
- **ValidatedBy**：自動從 JWT Token 取得管理者 ID
- **ValidatedAt**：驗票時間（自動記錄）

## 相關 API

此 API 為驗票流程的第二步（執行驗票）。完整驗票流程包括：

1. **第一支 API**: `GET /api/admin/tickets/scan` - 掃描票券資訊
2. **第二支 API**: `POST /api/admin/tickets/{ticketId}/validate` - 執行驗票 ✅

## 資料庫變更

### 新增資料表

執行了資料庫遷移 `AddTicketValidateLog`，創建以下資料表：

**TicketValidateLog**:
- `Id` (int, PK) - 驗票記錄 ID
- `TicketId` (int, FK) - 票券 ID
- `ValidatedAt` (datetime) - 驗票時間
- `ValidatedBy` (int, FK) - 驗票人員 ID
- `ValidationResult` (bit) - 驗票結果

## 實作狀態

- [x] API 設計與規劃
- [x] 程式碼實作
- [x] 資料庫遷移
- [x] 單元測試
- [x] 整合測試
- [x] 文檔撰寫
- [x] 部署準備

**狀態**: ✅ 已完成並測試通過

## 最後更新

- **日期**: 2025-12-30
- **版本**: v1.0.0
- **Commit**: `453b3ca`
