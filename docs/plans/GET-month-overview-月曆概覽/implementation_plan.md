# 月曆狀態 API 實作計畫

## 目標

實作 `GET /api/admin/daily-schedules/month-overview` API，用於獲取特定月份的所有日期狀態，供前端渲染月曆介面。

### 功能需求
- 查詢參數：年份（year）和月份（month）
- 回傳該月份中所有有 `DailySchedule` 記錄的日期及其狀態
- 只返回有記錄的日期，沒有記錄的日期不回傳（前端判斷為無點）
- 支援跨月份查詢，提供月曆切換功能

---

## User Review Required

> [!IMPORTANT]
> ### API 設計決策
> 
> **查詢參數格式**：使用 `year` 和 `month` 兩個獨立的整數參數，而非單一字串參數
> - ✅ **優點**：類型安全、驗證簡單、符合 RESTful 最佳實踐
> - **範例**：`GET /api/admin/daily-schedules/month-overview?year=2025&month=12`
> 
> **回傳資料最小化**：僅返回日期和狀態，不包含場次詳細資訊
> - ✅ **優點**：減少資料傳輸量、提升效能、符合單一職責原則
> - 如需場次詳細資訊，使用既有的 `GET /api/admin/daily-schedules/{date}` API

---

## Proposed Changes

### DTO Layer

#### [NEW] [MonthOverviewRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MonthOverviewRequestDto.cs)

建立請求 DTO，包含查詢參數驗證：

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 月曆概覽查詢請求 DTO
/// </summary>
public class MonthOverviewRequestDto
{
    /// <summary>
    /// 年份（例如：2025）
    /// </summary>
    /// <example>2025</example>
    [Range(2000, 2100, ErrorMessage = "年份必須在 2000 到 2100 之間")]
    public int Year { get; set; }

    /// <summary>
    /// 月份（1-12）
    /// </summary>
    /// <example>12</example>
    [Range(1, 12, ErrorMessage = "月份必須在 1 到 12 之間")]
    public int Month { get; set; }
}
```

#### [NEW] [MonthOverviewResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MonthOverviewResponseDto.cs)

建立回應 DTO：

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 月曆概覽回應 DTO
/// </summary>
public class MonthOverviewResponseDto
{
    /// <summary>
    /// 年份
    /// </summary>
    /// <example>2025</example>
    public int Year { get; set; }

    /// <summary>
    /// 月份
    /// </summary>
    /// <example>12</example>
    public int Month { get; set; }

    /// <summary>
    /// 該月份中有時刻表的日期清單
    /// </summary>
    public List<DailyScheduleStatusDto> Dates { get; set; } = new();
}

/// <summary>
/// 每日時刻表狀態 DTO
/// </summary>
public class DailyScheduleStatusDto
{
    /// <summary>
    /// 日期（格式：YYYY-MM-DD）
    /// </summary>
    /// <example>2025-12-01</example>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 狀態：Draft（草稿）或 OnSale（販售中）
    /// </summary>
    /// <example>OnSale</example>
    public string Status { get; set; } = string.Empty;
}
```

---

### Repository Layer

#### [MODIFY] [IDailyScheduleRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IDailyScheduleRepository.cs)

新增方法簽章：

```csharp
/// <summary>
/// 獲取指定月份的所有時刻表
/// </summary>
/// <param name="year">年份</param>
/// <param name="month">月份（1-12）</param>
/// <returns>該月份的所有時刻表記錄</returns>
Task<List<DailySchedule>> GetByMonthAsync(int year, int month);
```

#### [MODIFY] [DailyScheduleRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/DailyScheduleRepository.cs)

實作查詢方法：

```csharp
/// <inheritdoc />
public async Task<List<DailySchedule>> GetByMonthAsync(int year, int month)
{
    // 計算該月份的第一天和最後一天
    var startDate = new DateTime(year, month, 1);
    var endDate = startDate.AddMonths(1).AddDays(-1);

    return await _context.DailySchedules
        .Where(ds => ds.ScheduleDate >= startDate && ds.ScheduleDate <= endDate)
        .OrderBy(ds => ds.ScheduleDate)
        .ToListAsync();
}
```

---

### Service Layer

#### [MODIFY] [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs)

新增服務方法簽章：

```csharp
/// <summary>
/// 獲取月曆概覽
/// </summary>
/// <param name="year">年份</param>
/// <param name="month">月份（1-12）</param>
/// <returns>該月份的所有日期狀態</returns>
Task<MonthOverviewResponseDto> GetMonthOverviewAsync(int year, int month);
```

#### [MODIFY] [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)

實作業務邏輯：

```csharp
/// <inheritdoc />
public async Task<MonthOverviewResponseDto> GetMonthOverviewAsync(int year, int month)
{
    // 從 Repository 獲取該月份的所有時刻表
    var schedules = await _dailyScheduleRepository.GetByMonthAsync(year, month);

    // 轉換為 DTO
    var dates = schedules.Select(s => new DailyScheduleStatusDto
    {
        Date = s.ScheduleDate.ToString("yyyy-MM-dd"),
        Status = s.Status
    }).ToList();

    return new MonthOverviewResponseDto
    {
        Year = year,
        Month = month,
        Dates = dates
    };
}
```

---

### Controller Layer

#### [MODIFY] [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

新增 API 端點，加入完整的 XML 文件註解以供 Scalar 顯示：

```csharp
/// <summary>
/// /api/admin/daily-schedules/month-overview 獲取月曆概覽
/// </summary>
/// <remarks>
/// 查詢指定月份中所有有時刻表記錄的日期及其狀態。
/// 
/// **用途**：
/// - 前端顯示月曆介面，標註不同狀態的日期
/// - 黃點：狀態為 OnSale（販售中）
/// - 灰點：狀態為 Draft（草稿）
/// - 無點：該日期沒有時刻表記錄（不在回傳資料中）
/// 
/// **回傳資料**：
/// - 只返回有 DailySchedule 記錄的日期
/// - 沒有記錄的日期不回傳，由前端判斷為無點
/// 
/// **範例請求**：
/// ```
/// GET /api/admin/daily-schedules/month-overview?year=2025&amp;month=12
/// ```
/// 
/// **範例回應**：
/// ```json
/// {
///   "year": 2025,
///   "month": 12,
///   "dates": [
///     { "date": "2025-12-01", "status": "OnSale" },
///     { "date": "2025-12-10", "status": "Draft" },
///     { "date": "2025-12-25", "status": "OnSale" }
///   ]
/// }
/// ```
/// </remarks>
/// <param name="year">年份（例如：2025）</param>
/// <param name="month">月份（1-12）</param>
/// <response code="200">查詢成功</response>
/// <response code="400">參數錯誤（年份或月份不合法）</response>
/// <response code="401">未授權</response>
[HttpGet("month-overview")]
[ProducesResponseType(typeof(MonthOverviewResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> GetMonthOverview(
    [FromQuery] int year,
    [FromQuery] int month)
{
    // 參數驗證
    if (year < 2000 || year > 2100)
    {
        return BadRequest(new { message = "年份必須在 2000 到 2100 之間" });
    }

    if (month < 1 || month > 12)
    {
        return BadRequest(new { message = "月份必須在 1 到 12 之間" });
    }

    var result = await _dailyScheduleService.GetMonthOverviewAsync(year, month);
    return Ok(result);
}
```

---

## Verification Plan

### Automated Tests

建立 HTTP 測試檔案 `docs/tests/月曆概覽API/test-month-overview.http`：

#### 測試案例 1：查詢有資料的月份
```http
### 查詢 2025 年 12 月的月曆概覽（成功）
GET {{baseUrl}}/api/admin/daily-schedules/month-overview?year=2025&month=12
Authorization: Bearer {{adminToken}}

### 預期結果：
### - Status: 200 OK
### - 回傳該月份所有有時刻表的日期及狀態
```

#### 測試案例 2：查詢沒有資料的月份
```http
### 查詢 2026 年 1 月的月曆概覽（空資料）
GET {{baseUrl}}/api/admin/daily-schedules/month-overview?year=2026&month=1
Authorization: Bearer {{adminToken}}

### 預期結果：
### - Status: 200 OK
### - dates 為空陣列
```

#### 測試案例 3：參數驗證 - 非法年份
```http
### 無效年份（應返回 400）
GET {{baseUrl}}/api/admin/daily-schedules/month-overview?year=1999&month=12
Authorization: Bearer {{adminToken}}

### 預期結果：
### - Status: 400 Bad Request
### - message: "年份必須在 2000 到 2100 之間"
```

#### 測試案例 4：參數驗證 - 非法月份
```http
### 無效月份（應返回 400）
GET {{baseUrl}}/api/admin/daily-schedules/month-overview?year=2025&month=13
Authorization: Bearer {{adminToken}}

### 預期結果：
### - Status: 400 Bad Request
### - message: "月份必須在 1 到 12 之間"
```

#### 測試案例 5：未授權訪問
```http
### 未授權訪問（應返回 401）
GET {{baseUrl}}/api/admin/daily-schedules/month-overview?year=2025&month=12

### 預期結果：
### - Status: 401 Unauthorized
```

### Manual Verification

執行測試步驟：
1. 啟動本地開發伺服器
2. 使用 Admin 帳號登入獲取 Token
3. 執行所有測試案例，確認回應符合預期
4. 在 Scalar UI 中檢查 API 文件是否正確顯示
5. 前端整合測試（如已實作前端月曆介面）
