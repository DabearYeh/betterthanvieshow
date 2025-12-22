# POST /api/admin/showtimes API 實作計畫

實作「新增場次」API，讓管理者可以在某日期新增電影放映場次。

## User Review Required

> [!IMPORTANT]
> 此 API 需要建立兩個新的 Entity (`MovieShowTime`、`DailySchedule`)，請確認資料模型設計符合需求。

---

## Proposed Changes

### Entity Layer (Models/Entities)

#### [NEW] [DailySchedule.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/DailySchedule.cs)
每日時刻表實體：
- `Id` (int, PK)
- `ScheduleDate` (DateTime, Unique) - 日期
- `Status` (string) - Draft / OnSale
- `CreatedAt`, `UpdatedAt` (DateTime)

#### [NEW] [MovieShowTime.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/MovieShowTime.cs)
場次實體：
- `Id` (int, PK)
- `MovieId` (int, FK to Movie)
- `TheaterId` (int, FK to Theater)
- `ShowDate` (DateTime) - 放映日期
- `StartTime` (TimeSpan) - 開始時間（HH:MM）
- Navigation: `Movie`, `Theater`

---

### DTO Layer (Models/DTOs)

#### [NEW] [CreateShowtimeRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CreateShowtimeRequestDto.cs)
```csharp
{
    "movieId": 1,
    "theaterId": 1,
    "showDate": "2025-12-25",
    "startTime": "14:00"
}
```

#### [NEW] [ShowtimeResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/ShowtimeResponseDto.cs)
回傳場次完整資訊（含電影名稱、影廳名稱）

---

### Repository Layer

#### [NEW] [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs)
```csharp
Task<MovieShowTime> CreateAsync(MovieShowTime showtime);
Task<bool> HasTimeConflictAsync(int theaterId, DateTime showDate, TimeSpan startTime, int duration);
```

#### [NEW] [IDailyScheduleRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IDailyScheduleRepository.cs)
```csharp
Task<DailySchedule?> GetByDateAsync(DateTime date);
Task<DailySchedule> CreateAsync(DailySchedule schedule);
```

#### [NEW] [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs)
#### [NEW] [DailyScheduleRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/DailyScheduleRepository.cs)

---

### Service Layer

#### [NEW] [IShowtimeService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IShowtimeService.cs)
```csharp
Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeRequestDto dto);
```

#### [NEW] [ShowtimeService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/ShowtimeService.cs)
業務邏輯：
1. 驗證 movieId 存在且在上映期間內
2. 驗證 theaterId 存在
3. 驗證 startTime 是 15 分鐘的倍數
4. 檢查 DailySchedule 狀態（OnSale 禁止新增）
5. 若日期無 DailySchedule，自動建立 Draft 狀態
6. 檢查時間衝突
7. 建立 MovieShowTime

---

### Controller Layer

#### [NEW] [ShowtimesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/ShowtimesController.cs)
```http
POST /api/admin/showtimes
Authorization: Bearer {admin_token}
```

---

### Database

#### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)
- 新增 `DbSet<MovieShowTime>` 和 `DbSet<DailySchedule>`
- 配置 Entity 約束與索引

#### [NEW] Migration
執行 `dotnet ef migrations add AddShowtimeAndDailySchedule`

---

### DI Registration

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)
註冊新的 Repository 和 Service

---

## Verification Plan

### HTTP Tests (betterthanvieshow.http)

新增以下測試案例：

| 測試案例 | 預期結果 |
|----------|----------|
| 成功新增場次（含自動建立 DailySchedule） | 201 Created |
| 電影不存在 | 400 Bad Request |
| 影廳不存在 | 400 Bad Request |
| 時間不是 15 分鐘倍數 | 400 Bad Request |
| 日期在電影上映範圍外 | 400 Bad Request |
| 時間衝突 | 409 Conflict |
| DailySchedule 為 OnSale 時禁止新增 | 403 Forbidden |
| 未授權 | 401 Unauthorized |

### 執行步驟

```powershell
# 1. 新增 Migration
dotnet ef migrations add AddShowtimeAndDailySchedule --project betterthanvieshow

# 2. 更新資料庫
dotnet ef database update --project betterthanvieshow

# 3. 啟動應用程式
dotnet run --project betterthanvieshow

# 4. 使用 VS Code REST Client 或 Postman 測試 HTTP 請求
```
