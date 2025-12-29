# GET /api/orders/{id} 實作指南

## 實作摘要

實作訂單詳情查詢 API，用於「訂單確認與支付選擇」頁面。此 API 提供完整訂單資訊，包含電影、場次、影廳、座位及金額明細，並確保使用者只能查詢自己的訂單。

## 1. DTO 設計

### [OrderDetailResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/OrderDetailResponseDto.cs) （新增）

設計了完整的回應結構，包含：
- `OrderDetailResponseDto`：主要回應 DTO
- `OrderMovieInfoDto`：電影資訊（标題、分級、片長、海報）
- `OrderShowtimeInfoDto`：場次資訊（日期、時間、星期幾）
- `OrderTheaterInfoDto`：影廳資訊（名稱、類型）
- 重用現有的 `SeatInfoDto`：座位與票券資訊

**設計重點**：
- 不包含手續費，`TotalAmount` 直接等於訂單的 `TotalPrice`
- 中文星期幾（一、二、三...日）由後端計算並提供
- StartTime 格式化為 `HH:mm`（例：16:30）

## 2. Repository Layer

**無需修改**：現有的 `OrderRepository.GetByIdAsync` 已經包含所有需要的 Include關聯：
- Movie
- Theater
- Tickets → Seat

## 3. Service Layer

### [IOrderService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IOrderService.cs)
新增方法：
```csharp
Task<OrderDetailResponseDto?> GetOrderDetailAsync(int orderId, int userId);
```

### [OrderService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/OrderService.cs)

實作要點：
1. **權限驗證**：檢查 `order.UserId == userId`
2. **資料映射**：組裝所有內嵌 DTO
3. **日期格式化**：
   - Date: `yyyy-MM-dd`
   - StartTime: `hh:mm`
4. **星期轉換**：使用 `GetChineseDayOfWeek` 輔助方法

**安全性設計**：
- 訂單不存在：回傳 `null`
- 無權查看（不是自己的訂單）：也回傳 `null`
- 在 Controller 層統一回傳 404，避免透露訂單是否存在

## 4. Controller Layer

### [OrdersController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/OrdersController.cs)

新增端點：
```csharp
[HttpGet("{id}")]
[Authorize]
public async Task<IActionResult> GetOrderDetail(int id)
```

**實作重點**：
- 從 JWT Token 解析 `userId`
- 呼叫 Service 層取得訂單詳情
- 錯誤處理：
  - 無效 Token → 401
  - 訂單不存在或無權查看 → 404（統一回應，資安考量）
  - 其他例外 → 500

**API 文檔**：
- 完整的 XML 註解
- 標註為訂票流程的「第五步」
- 詳細說明安全性要求與回應內容

## 5. 金額明細設計

根據使用者確認，系統**不收取手續費**：
- `TotalAmount` = 訂單的 `TotalPrice`（即票價總計）
- 不需要 `OrderPricingDto`
- 簡化了實作與測試

## 6. 技術亮點

### 星期轉換
實作 `GetChineseDayOfWeek` 方法，將 `DayOfWeek` 枚舉轉換為中文：
```csharp
DayOfWeek.Monday => "一"
DayOfWeek.Tuesday => "二"
...
```

### 安全性考量
- **權限隔離**：使用者只能查看自己的訂單
- **資訊洩漏防護**：不區分「訂單不存在」與「無權查看」，統一回傳 404
- **Token 驗證**：在 Controller 層明確檢查 JWT Claims

## 7. 測試建議

建議測試場景：
1. ✅ **正常查詢**：使用者查詢自己的訂單
2. ✅ **訂單不存在**：查詢不存在的訂單 ID
3. ✅ **權限檢查**：嘗試查詢他人的訂單
4. ✅ **未登入**：不帶 Token 查詢

## 8. 前端整合

前端可透過此 API 取得：
- 倒數計時：使用 `ExpiresAt` 計算剩餘秒數
- 電影海報：`movie.posterUrl`
- 場次時間：`showtime.date` + `showtime.dayOfWeek` + `showtime.startTime`
- 座位列表：`seats` 陣列
- 應付金額：`totalAmount`
