# PUT /api/admin/daily-schedules/{date} API 實作計畫

實作「儲存每日時刻表」API，讓管理者可以一次儲存某日期的所有場次資料。

## User Review Required

> [!IMPORTANT]
> 此 API 使用「全刪全建」策略處理批次儲存，請確認這符合您的業務需求。

> [!WARNING]
> 此 API 會採用 **Upsert** 邏輯（有則更新，無則新建），不管是第一次還是第 N 次儲存，都使用同一支 API。

---

## Proposed Changes

### DTO Layer

#### [NEW] [SaveDailyScheduleRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/SaveDailyScheduleRequestDto.cs)
請求格式（場次清單）：
```json
{
  "showtimes": [
    { "movieId": 1, "theaterId": 1, "startTime": "09:45" },
    { "movieId": 2, "theaterId": 2, "startTime": "14:00" }
  ]
}
```

#### [NEW] [ShowtimeItemDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/ShowtimeItemDto.cs)
單一場次資料結構：
- `MovieId` (int)
- `TheaterId` (int)
- `StartTime` (string) - 格式 HH:MM，必須是 15 分鐘倍數

#### [NEW] [DailyScheduleResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/DailyScheduleResponseDto.cs)
回傳時刻表完整資訊：
- `ScheduleDate` (DateTime)
- `Status` (string) - Draft / OnSale
- `Showtimes` (List<ShowtimeResponseDto>) - 該日期所有場次
- `CreatedAt`, `UpdatedAt` (DateTime)

---

### Repository Layer

#### [MODIFY] [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs)
新增批次操作方法：
```csharp
// 刪除指定日期的所有場次
Task DeleteByDateAsync(DateTime date);

// 批次新增場次
Task<List<MovieShowTime>> CreateBatchAsync(List<MovieShowTime> showtimes);

// 取得指定日期的所有場次（含關聯資料）
Task<List<MovieShowTime>> GetByDateWithDetailsAsync(DateTime date);
```

#### [MODIFY] [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs)
實作批次操作方法

---

### Service Layer

#### [NEW] [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs)
```csharp
Task<DailyScheduleResponseDto> SaveDailyScheduleAsync(DateTime date, SaveDailyScheduleRequestDto dto);
```

#### [NEW] [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)

**業務邏輯（重要）**：

1. **開啟資料庫交易 (Transaction)**
2. **檢查或建立 DailySchedule**：
   - 若不存在 → 建立新的（Draft 狀態）
   - 若存在且為 OnSale → 拋出 403 錯誤
3. **驗證所有場次資料**：
   - 電影存在且在上映期間內
   - 影廳存在
   - 開始時間為 15 分鐘倍數
   - 檢查場次清單內部的時間衝突
4. **全刪全建**：
   - 刪除該日期所有舊場次
   - 批次新增新場次
5. **更新 DailySchedule.UpdatedAt**
6. **提交交易 (Commit)**
7. **載入完整資料並回傳**

---

### Controller Layer

#### [NEW] [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

```http
PUT /api/admin/daily-schedules/{date}
Authorization: Bearer {admin_token}
```

**完整 XML 註解範例**：
```csharp
/// <summary>
/// 儲存每日時刻表
/// </summary>
/// <remarks>
/// 用來新增或修改特定日期的時刻表。
/// 
/// - 第一次儲存：自動建立 DailySchedule（Draft 狀態）
/// - 後續儲存：更新場次資料（全部替換）
/// - 已販售的時刻表無法修改
/// 
/// **範例請求**：
/// ```json
/// {
///   "showtimes": [
///     { "movieId": 1, "theaterId": 1, "startTime": "09:45" },
///     { "movieId": 2, "theaterId": 2, "startTime": "14:00" }
///   ]
/// }
/// ```
/// </remarks>
/// <param name="date">時刻表日期，格式：YYYY-MM-DD</param>
/// <param name="request">時刻表資料</param>
/// <response code="200">儲存成功</response>
/// <response code="400">參數錯誤</response>
/// <response code="403">該日期已開始販售</response>
/// <response code="409">場次時間衝突</response>
```

---

### DI Registration

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)
```csharp
builder.Services.AddScoped<IDailyScheduleService, DailyScheduleService>();
```

---

## Verification Plan

### HTTP Tests (betterthanvieshow.http)

| 測試案例 | 預期結果 |
|----------|----------|
| 首次儲存時刻表（空白 → 有資料） | 200 OK，自動建立 DailySchedule (Draft) |
| 修改時刻表（Draft 狀態） | 200 OK，場次全部替換 |
| 儲存空清單（清空該日期所有場次） | 200 OK，保留 DailySchedule (Draft) |
| 嘗試修改已販售時刻表 | 403 Forbidden |
| 電影不存在 | 400 Bad Request |
| 影廳不存在 | 400 Bad Request |
| 時間非 15 分鐘倍數 | 400 Bad Request |
| 場次清單內部時間衝突 | 409 Conflict |
| 未授權 | 401 Unauthorized |

### 測試步驟

```powershell
# 1. 啟動應用程式
dotnet run --project betterthanvieshow

# 2. 使用 betterthanvieshow.http 進行測試
```

---

## 注意事項

1. **Transaction 非常重要**：必須確保「刪除舊場次」和「新增新場次」在同一個 Transaction 內，避免資料不一致。
2. **時間衝突檢查**：需要檢查傳入的場次清單內部是否有衝突，不能依賴資料庫約束。
3. **空清單處理**：如果傳入空陣列，代表清空該日期所有場次，但保留 DailySchedule 記錄。
