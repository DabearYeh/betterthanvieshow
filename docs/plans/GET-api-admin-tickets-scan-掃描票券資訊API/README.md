# GET /api/admin/tickets/scan - 掃描票券資訊 API

## API 概述

此 API 用於管理者掃描顧客出示的票券 QR Code，查詢並顯示票券詳細資訊。

- **端點**: `GET /api/admin/tickets/scan?qrCode={ticketNumber}`
- **授權**: 需要 Admin 角色
- **功能**: 查詢票券資訊（不執行驗票，不修改狀態）

## 資料夾結構

```
GET-api-admin-tickets-scan-掃描票券資訊API/
├── README.md                    # 本檔案
├── implementation_plan.md       # 實作計劃
├── walkthrough.md              # 驗收報告
└── tests/                      # 測試相關
    ├── test-scan-ticket.http   # HTTP 測試腳本
    └── test_results.md         # 測試結果報告
```

## 文檔說明

### 1. implementation_plan.md
實作計劃文件，包含：
- API 設計規格
- 資料模型（DTO）
- Repository、Service、Controller 層設計
- 驗證計劃

### 2. walkthrough.md
驗收報告文件，包含：
- 已完成功能摘要
- 程式碼變更清單
- 測試指引
- 驗證清單

### 3. tests/test-scan-ticket.http
HTTP 測試腳本，包含：
- 掃描有效票券
- 掃描不存在的票券
- 掃描各種狀態的票券
- 授權測試

### 4. tests/test_results.md
測試結果報告，記錄：
- 所有測試案例執行結果
- 實際 API 回應
- 功能驗證清單

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
     "email": "admin@betterthanvieshow.com",
     "password": "Admin@123"
   }
   ```

### 測試 API

使用 HTTP 測試工具執行：
```http
GET http://localhost:5041/api/admin/tickets/scan?qrCode={票券編號}
Authorization: Bearer {你的_admin_token}
```

## API 回應範例

### 成功回應 (200 OK)
```json
{
  "success": true,
  "message": "成功取得票券資訊",
  "data": {
    "ticketId": 52,
    "ticketNumber": "82253783",
    "status": "Unused",
    "movieTitle": "復仇者聯盟",
    "showDate": "2025-12-31",
    "showTime": "10:00",
    "seatRow": "A",
    "seatColumn": 1,
    "seatLabel": "A 排 1 號",
    "theaterName": "IMAX 3D Theatre",
    "theaterType": "IMAX"
  }
}
```

### 錯誤回應 (404 Not Found)
```json
{
  "message": "票券不存在"
}
```

## 相關 API

此 API 為驗票流程的第一步（查詢票券資訊）。完整驗票流程需要搭配：

- **第二支 API**: `POST /api/admin/tickets/{ticketId}/validate` - 執行驗票並更新狀態

## 實作狀態

- [x] API 設計與規劃
- [x] 程式碼實作
- [x] 單元測試
- [x] 整合測試
- [x] 文檔撰寫
- [x] 部署準備

**狀態**: ✅ 已完成並測試通過

## 最後更新

- **日期**: 2025-12-30
- **版本**: v1.0.0
- **作者**: Gemini AI Assistant
