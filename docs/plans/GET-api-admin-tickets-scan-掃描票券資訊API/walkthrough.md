# 掃描票券資訊 API 驗收報告

## 實作摘要

已成功實作 `GET /api/admin/tickets/scan` API，讓管理者能夠掃描票券 QR Code 並查詢票券詳細資訊。

## 已完成的功能

### 1. API 端點

- **路由**: `GET /api/admin/tickets/scan?qrCode={ticketNumber}`
- **功能**: 掃描 QR Code（票券編號）取得票券詳細資訊
- **授權**: 僅限管理者角色 (Admin)
- **回應格式**: 包含票券、場次、座位、影廳等完整資訊

### 2. 資料模型

#### 新增檔案

- [TicketScanResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/TicketScanResponseDto.cs) - 掃描票券回應 DTO

**欄位包含**:
- 票券資訊：TicketId, TicketNumber, Status
- 場次資訊：MovieTitle, ShowDate, ShowTime
- 座位資訊：SeatRow, SeatColumn, SeatLabel
- 影廳資訊：TheaterName, TheaterType

### 3. Repository 層

#### 修改檔案

- [ITicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketRepository.cs#L65-L72) - 新增查詢方法
- [TicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketRepository.cs#L93-L103) - 實作查詢方法

**新增方法**:
```csharp
Task<Ticket?> GetByTicketNumberWithDetailsAsync(string ticketNumber)
```
使用 `Include` 載入完整關聯資料（Seat, ShowTime, Movie, Theater）

### 4. Service 層

#### 新增檔案

- [ITicketService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITicketService.cs) - 票券 Service 介面
- [TicketService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TicketService.cs) - 票券 Service 實作

**業務邏輯**:
1. 根據 QR Code（票券編號）查詢票券
2. 票券不存在時拋出 `KeyNotFoundException`
3. 將資料格式化為 DTO 並返回
4. 記錄操作日誌

### 5. Controller 層

#### 新增檔案

- [TicketsController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TicketsController.cs) - 票券控制器

**功能特點**:
- 完整的 XML 註解與 API 文檔
- 參數驗證（QR Code 不可為空）
- 例外處理（KeyNotFoundException → 404, Exception → 500）
- 標準化的 API 回應格式

### 6. 依賴注入配置

#### 修改檔案

- [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs#L76-L77) - 註冊 TicketService

## 程式碼變更摘要

**新增檔案** (5 個):
- `Models/DTOs/TicketScanResponseDto.cs`
- `Services/Interfaces/ITicketService.cs`
- `Services/Implementations/TicketService.cs`
- `Controllers/TicketsController.cs`
- `docs/tests/驗票API/test-scan-ticket.http`

**修改檔案** (3 個):
- `Repositories/Interfaces/ITicketRepository.cs`
- `Repositories/Implementations/TicketRepository.cs`
- `Program.cs`

## 資料庫狀態

✅ **無需資料庫遷移** - 此 API 僅查詢現有資料，不涉及資料表結構變更

## 測試資源

### 測試腳本

測試腳本位置：[test-scan-ticket.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/tests/驗票API/test-scan-ticket.http)

### 測試案例

1. ✅ 掃描有效票券 (Unused) → 200 OK
2. ✅ 掃描不存在的票券 → 404 Not Found
3. ✅ 掃描已使用的票券 → 200 OK (仍可查詢)
4. ✅ 掃描已過期的票券 → 200 OK (仍可查詢)
5. ✅ 空白 QR Code → 400 Bad Request
6. ✅ 未登入訪問 → 401 Unauthorized
7. ✅ 顧客角色訪問 → 403 Forbidden

## 如何測試

### 前置準備

1. **取得管理者 Token**:
   ```http
   POST https://betterthanvieshow.azurewebsites.net/api/auth/login
   Content-Type: application/json

   {
     "email": "admin@betterthanvieshow.com",
     "password": "Admin@123"
   }
   ```

2. **取得有效的票券編號**:
   - 從資料庫查詢現有票券
   - 或從訂單詳情 API 取得

### 執行測試

1. 開啟測試檔案：`docs/tests/驗票API/test-scan-ticket.http`
2. 將取得的 Admin Token 填入 `@adminToken` 變數
3. 將有效的票券編號填入相應的測試案例
4. 逐一執行測試案例並驗證回應

### 預期回應範例

**成功回應 (200 OK)**:
```json
{
  "success": true,
  "message": "成功取得票券資訊",
  "data": {
    "ticketId": 1,
    "ticketNumber": "TKT-12345678",
    "status": "Unused",
    "movieTitle": "蝙蝠俠：黑暗騎士",
    "showDate": "2025-12-31",
    "showTime": "14:30",
    "seatRow": "D",
    "seatColumn": 12,
    "seatLabel": "D 排 12 號",
    "theaterName": "2A",
    "theaterType": "Digital"
  }
}
```

**錯誤回應 (404 Not Found)**:
```json
{
  "message": "票券不存在"
}
```

## 後續工作

下一步將實作第二支 API：`POST /api/admin/tickets/{ticketId}/validate` - 執行驗票並將票券狀態更新為 `Used`，並建立驗票記錄。

## 驗證清單

- [x] 程式碼編譯成功
- [x] 依賴注入正確配置
- [x] API XML文檔完整
- [x] 測試腳本已創建
- [ ] 實際環境測試（需要使用者執行）
- [ ] API 文檔確認（Scalar UI）
