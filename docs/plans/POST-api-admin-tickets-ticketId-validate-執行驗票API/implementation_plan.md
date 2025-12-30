# 執行驗票 API 實作計劃

## 目標

實作 `POST /api/admin/tickets/{ticketId}/validate` API，讓管理者能夠執行驗票，將票券狀態從 `Unused` 更新為 `Used`，並建立驗票記錄。

### 功能需求

根據上傳的 UI 圖片和規格文件，驗票成功後會顯示「驗票成功！」的對話框。

### 業務規則（來自 `驗票.feature`）

1. **票券狀態檢查**：
   - `Unused` → 允許驗票 → 更新為 `Used`
   - `Used` → 拒絕驗票 → 回傳「票券已使用」
   - `Expired` → 拒絕驗票 → 回傳「票券已過期」
   - `Pending` → 拒絕驗票 → 回傳「票券未支付」
   - 不存在 → 拒絕驗票 → 回傳「票券不存在」

2. **驗票人員識別**：
   - 自動從登入的 JWT Token 取得管理者 ID
   - 寫入 `TicketValidateLog.ValidatedBy`

3. **驗票記錄**：
   - 每次驗票嘗試都必須建立 `TicketValidateLog` 記錄
   - 成功：`ValidationResult = true`
   - 失敗：`ValidationResult = false`

## 需要使用者確認的事項

> [!WARNING]
> **驗票失敗時是否建立記錄？**
> 
> 根據規格文件，「每次驗票嘗試都必須建立記錄」，包含失敗的情況。
> 
> 但考慮到實務需求，有兩種設計選擇：
> 
> **方案一（完全按照規格）**：
> - 任何驗票嘗試（成功或失敗）都建立 `TicketValidateLog` 記錄
> - 優點：完整記錄所有驗票行為，可追蹤異常嘗試
> - 缺點：資料庫記錄較多，包含無效的嘗試
> 
> **方案二（簡化設計）**：
> - 僅在驗票成功時建立 `TicketValidateLog` 記錄
> - 失敗時僅回傳錯誤訊息，不寫入資料庫
> - 優點：資料更乾淨，僅記錄有效的驗票
> - 缺點：無法追蹤失敗的嘗試
> 
> **建議採用方案一**（按照規格），但需要您確認是否同意。

## 建議變更

### 元件一：資料模型

#### [NEW] [TicketValidateLog.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/TicketValidateLog.cs)

創建 `TicketValidateLog` 實體類別：

```csharp
public class TicketValidateLog
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public int ValidatedBy { get; set; }
    public bool ValidationResult { get; set; }
    
    // Navigation Properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual User Validator { get; set; } = null!;
}
```

---

### 元件二：Repository 層

#### [NEW] [ITicketValidateLogRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketValidateLogRepository.cs)

創建驗票記錄 Repository 介面：

```csharp
public interface ITicketValidateLogRepository
{
    Task<TicketValidateLog> CreateAsync(TicketValidateLog log);
}
```

#### [NEW] [TicketValidateLogRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketValidateLogRepository.cs)

實作驗票記錄 Repository。

#### [MODIFY] [ITicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketRepository.cs)

新增方法：

```csharp
/// <summary>
/// 根據票券 ID 取得票券（不含關聯資料）
/// </summary>
Task<Ticket?> GetByIdAsync(int ticketId);
```

#### [MODIFY] [TicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketRepository.cs)

實作 `GetByIdAsync` 方法。

---

### 元件三：Service 層

#### [MODIFY] [ITicketService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITicketService.cs)

新增方法：

```csharp
/// <summary>
/// 執行驗票
/// </summary>
/// <param name="ticketId">票券 ID</param>
/// <param name="validatedBy">驗票人員 ID（管理者）</param>
Task ValidateTicketAsync(int ticketId, int validatedBy);
```

#### [MODIFY] [TicketService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TicketService.cs)

實作 `ValidateTicketAsync` 方法，業務邏輯：

1. 查詢票券是否存在
   - 不存在 → 拋出 `KeyNotFoundException("票券不存在")`
   
2. 檢查票券狀態：
   - `Pending` → 拋出 `InvalidOperationException("票券未支付")`
   - `Used` → 拋出 `InvalidOperationException("票券已使用")`
   - `Expired` → 拋出 `InvalidOperationException("票券已過期")`
   - `Unused` → 繼續處理
   
3. 更新票券狀態為 `Used`

4. 建立 `TicketValidateLog` 記錄（`ValidationResult = true`）

5. **例外處理**：
   - 若方案一被採納，需在 catch 區塊建立失敗的 `TicketValidateLog` 記錄（`ValidationResult = false`）

---

### 元件四：Controller 層

#### [MODIFY] [TicketsController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TicketsController.cs)

新增端點：

```csharp
/// <summary>
/// POST /api/admin/tickets/{ticketId}/validate 執行驗票
/// </summary>
[HttpPost("{ticketId}/validate")]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
[ProducesResponseType(404)]
public async Task<IActionResult> ValidateTicket(int ticketId)
{
    // 從 JWT Token 取得管理者 ID
    var validatedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    
    // 執行驗票
    await _ticketService.ValidateTicketAsync(ticketId, validatedBy);
    
    return Ok(new { message = "驗票成功" });
}
```

**API 文檔摘要**：
- 路由：`POST /api/admin/tickets/{ticketId}/validate`
- 參數：`ticketId` (int, required, from route)
- 回應：
  - `200 OK`：驗票成功，票券狀態已更新為 Used
  - `400 Bad Request`：票券狀態不允許驗票（已使用/已過期/未支付）
  - `404 Not Found`：票券不存在
  - `401 Unauthorized`：未登入
  - `403 Forbidden`：非管理者角色

---

### 元件五：資料庫配置

#### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)

1. 新增 `DbSet<TicketValidateLog> TicketValidateLogs`

2. 在 `OnModelCreating` 中配置 `TicketValidateLog` 實體：

```csharp
modelBuilder.Entity<TicketValidateLog>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // 外鍵 - Ticket
    entity.HasOne(e => e.Ticket)
        .WithMany()
        .HasForeignKey(e => e.TicketId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // 外鍵 - User (驗票人員)
    entity.HasOne(e => e.Validator)
        .WithMany()
        .HasForeignKey(e => e.ValidatedBy)
        .OnDelete(DeleteBehavior.Restrict);
    
    // 驗票時間預設值
    entity.Property(e => e.ValidatedAt)
        .HasDefaultValueSql("GETDATE()");
});
```

---

### 元件六：資料庫遷移

執行資料庫遷移以建立 `TicketValidateLog` 表格：

```bash
cd betterthanvieshow
dotnet ef migrations add AddTicketValidateLog
dotnet ef database update
```

---

### 元件七：依賴注入配置

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)

註冊 Repository：

```csharp
builder.Services.AddScoped<ITicketValidateLogRepository, TicketValidateLogRepository>();
```

---

## 驗證計劃

### 自動化測試

#### 1. 資料庫遷移測試
```powershell
cd c:\Users\VivoBook\Desktop\betterthanvieshow\betterthanvieshow
dotnet ef migrations add AddTicketValidateLog
dotnet ef database update
```
確保 `TicketValidateLog` 表格成功建立。

### 手動測試

#### 2. HTTP API 測試

創建測試腳本 `docs/tests/驗票API/test-validate-ticket.http`，包含以下測試案例：

1. **管理者登入**：取得 Admin Token
2. **驗證 Unused 票券**：`POST /api/admin/tickets/{id}/validate` → 200 OK，狀態更新為 Used
3. **重複驗證已使用票券**：`POST /api/admin/tickets/{id}/validate` → 400 Bad Request（票券已使用）
4. **驗證已過期票券**：`POST /api/admin/tickets/{id}/validate` → 400 Bad Request（票券已過期）
5. **驗證未支付票券**：`POST /api/admin/tickets/{id}/validate` → 400 Bad Request（票券未支付）
6. **驗證不存在的票券**：`POST /api/admin/tickets/{id}/validate` → 404 Not Found
7. **未授權訪問測試**：不提供 Token → 401 Unauthorized
8. **顧客角色訪問測試**：使用 Customer Token → 403 Forbidden

執行測試後，需要驗證：
- 票券狀態是否正確更新為 `Used`
- `TicketValidateLog` 記錄是否正確建立
- 驗票人員 ID 是否正確記錄

#### 3. 資料庫驗證

執行驗票後，查詢資料庫確認：

```sql
-- 檢查票券狀態是否更新為 Used
SELECT * FROM Ticket WHERE Id = {測試票券ID};

-- 檢查驗票記錄是否正確建立
SELECT * FROM TicketValidateLog WHERE TicketId = {測試票券ID};
```

### 整合測試

完整的驗票流程測試：

1. 使用第一支 API 掃描票券：`GET /api/admin/tickets/scan?qrCode={ticketNumber}`
2. 確認票券狀態為 `Unused`
3. 使用第二支 API 執行驗票：`POST /api/admin/tickets/{ticketId}/validate`
4. 確認回應為 200 OK，訊息為「驗票成功」
5. 再次掃描同一票券，確認狀態已變為 `Used`
6. 再次嘗試驗票，確認回傳 400 Bad Request（票券已使用）
