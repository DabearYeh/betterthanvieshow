# 掃描票券資訊 API 實作計劃

## 目標

實作 `GET /api/admin/tickets/scan?qrCode={ticketNumber}` API，讓管理者掃描票券 QR Code 後能查詢票券詳細資訊並顯示在畫面上。

### 功能需求

根據上傳的 UI 圖片，API 需要返回以下資訊：
- **日期**：場次日期（格式：12/31 (日)）
- **場次**：場次時間（格式：下午 2:30）
- **影廳**：座位區域（例如：2A）
- **位置**：座位排列（例如：D 排 12 號）
- **票種**：影廳類型（例如：一般 2D）
- **票號**：票券編號（例如：13395332）

### 業務規則
- QR Code 內容為票券編號 (TicketNumber)
- 票券不存在時返回 404 Not Found
- 僅供管理者使用（需要 Admin 角色）
- 此 API 僅查詢資訊，不修改票券狀態

## 建議變更

### 元件一：DTO 模型

#### [NEW] [TicketScanResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/TicketScanResponseDto.cs)

創建掃描票券回應 DTO：

```csharp
public class TicketScanResponseDto
{
    // 票券資訊
    public int TicketId { get; set; }
    public string TicketNumber { get; set; }
    public string Status { get; set; }
    
    // 場次資訊
    public string MovieTitle { get; set; }
    public string ShowDate { get; set; }        // 格式：2025-12-31
    public string ShowTime { get; set; }        // 格式：14:30
    
    // 座位資訊
    public string SeatRow { get; set; }         // 例如：D
    public int SeatColumn { get; set; }         // 例如：12
    public string SeatLabel { get; set; }       // 例如：D 排 12 號
    
    // 影廳資訊
    public string TheaterName { get; set; }     // 例如：2A
    public string TheaterType { get; set; }     // 例如：Digital, IMAX, 4DX
}
```

---

### 元件二：Repository 層

#### [MODIFY] [ITicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketRepository.cs)

新增方法：

```csharp
/// <summary>
/// 根據票券編號查詢票券及完整關聯資料
/// </summary>
Task<Ticket?> GetByTicketNumberWithDetailsAsync(string ticketNumber);
```

#### [MODIFY] [TicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketRepository.cs)

實作查詢方法，使用 `Include` 載入關聯資料：
- `Ticket.Seat`
- `Ticket.ShowTime`
- `Ticket.ShowTime.Movie`
- `Ticket.ShowTime.Theater`

---

### 元件三：Service 層

#### [NEW] [ITicketService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITicketService.cs)

創建票券 Service 介面：

```csharp
public interface ITicketService
{
    Task<TicketScanResponseDto> ScanTicketByQrCodeAsync(string qrCode);
}
```

#### [NEW] [TicketService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TicketService.cs)

實作票券 Service，業務邏輯：
1. 透過 QR Code（票券編號）查詢票券
2. 若票券不存在，拋出 `NotFoundException("票券不存在")`
3. 將資料轉換為 `TicketScanResponseDto` 並返回

---

### 元件四：Controller 層

#### [NEW] [TicketsController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TicketsController.cs)

創建票券控制器：

```csharp
[ApiController]
[Route("api/admin/tickets")]
[Authorize(Roles = "Admin")]
[Tags("Admin - 驗票管理")]
public class TicketsController : ControllerBase
{
    [HttpGet("scan")]
    [ProducesResponseType(typeof(ApiResponse<TicketScanResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> ScanTicket([FromQuery] string qrCode)
    {
        // 實作掃描票券邏輯
    }
}
```

**API 文檔摘要**：
- 路由：`GET /api/admin/tickets/scan`
- 參數：`qrCode` (string, required, from query)
- 回應：
  - `200 OK`：成功返回票券資訊
  - `404 Not Found`：票券不存在
  - `401 Unauthorized`：未登入
  - `403 Forbidden`：非管理者角色

---

### 元件五：依賴注入配置

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)

註冊 Service：
```csharp
builder.Services.AddScoped<ITicketService, TicketService>();
```

---

## 驗證計劃

### 手動測試

創建測試腳本：`docs/tests/驗票API/test-scan-ticket.http`

**測試案例**：
1. 管理者登入取得 Token
2. 掃描有效票券（狀態為 Unused）
3. 掃描不存在的票券
4. 掃描已使用的票券（驗證是否仍能查詢）
5. 掃描已過期的票券（驗證是否仍能查詢）
6. 未授權訪問測試
7. 顧客角色訪問測試

**預期結果**：
- 有效票券返回完整資訊
- 不存在的票券返回 404
- 已使用/已過期的票券仍可查詢（僅查詢不驗證）
