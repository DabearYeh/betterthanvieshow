# 分組時刻表 API 實作計畫

## 目標

實作 `GET /api/admin/daily-schedules/{date}/grouped` API，為側邊欄提供按「電影 + 影廳類型」分組的時刻表資料，方便前端直接渲染。

## 功能需求

根據側邊欄 UI 設計，此 API 需要：

### 資料分組邏輯
1. **按電影分組**：每部電影作為一個主要項目
2. **按影廳類型分組**：每部電影下，按影廳類型（Digital、4DX、IMAX）分組場次
3. **時間範圍計算**：每個影廳類型組顯示「最早開始時間 - 最晚結束時間」

### 顯示資訊
- 電影海報 (PosterUrl)
- 電影名稱 (Title)
- 電影分級 (Rating) - G/PG/R 顯示為 0+/12+/18+
- 片長 (Duration) - 格式化為「X 小時 Y 分鐘」
- 影廳類型 (Digital/4DX/IMAX) - 轉換為中文顯示（數位/4DX/IMAX）
- 時間範圍（該影廳類型的所有場次的時間跨度）
- 具體場次時間列表

### UI 顯示範例
```
雲深不知處(數位)
片長 2 小時 23 分鐘          0+
09:00 09:45

雲深不知處(4DX)
片長 2 小時 23 分鐘          0+
09:15

仙逆(4DX)
片長 2 小時 27 分鐘          18+
09:30
```

---

## 實作變更

### DTO 層

#### [NEW] `GroupedDailyScheduleResponseDto.cs`

主回應 DTO：

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 分組時刻表回應 DTO（用於側邊欄顯示）
/// </summary>
public class GroupedDailyScheduleResponseDto
{
    /// <summary>
    /// 時刻表日期
    /// </summary>
    public DateTime ScheduleDate { get; set; }

    /// <summary>
    /// 狀態：Draft（草稿）、OnSale（販售中）
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 按電影分組的場次列表
    /// </summary>
    public List<MovieShowtimeGroupDto> MovieShowtimes { get; set; } = new();
}
```

#### [NEW] `MovieShowtimeGroupDto.cs`

電影分組 DTO：

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影場次分組 DTO
/// </summary>
public class MovieShowtimeGroupDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 電影海報 URL
    /// </summary>
    public string? PosterUrl { get; set; }

    /// <summary>
    /// 電影分級（G、PG、R）
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級顯示格式（0+、12+、18+）
    /// </summary>
    public string RatingDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 片長顯示格式（如：2 小時 23 分鐘）
    /// </summary>
    public string DurationDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 按影廳類型分組的場次
    /// </summary>
    public List<TheaterTypeGroupDto> TheaterTypeGroups { get; set; } = new();
}
```

#### [NEW] `TheaterTypeGroupDto.cs`

影廳類型分組 DTO：

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 影廳類型分組 DTO
/// </summary>
public class TheaterTypeGroupDto
{
    /// <summary>
    /// 影廳類型（Digital、4DX、IMAX）
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型顯示名稱（數位、4DX、IMAX）
    /// </summary>
    public string TheaterTypeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 時間範圍（最早開始 - 最晚結束）
    /// </summary>
    public string TimeRange { get; set; } = string.Empty;

    /// <summary>
    /// 該影廳類型的所有場次
    /// </summary>
    public List<ShowtimeSimpleDto> Showtimes { get; set; } = new();
}
```

#### [NEW] `ShowtimeSimpleDto.cs`

簡化的場次 DTO：

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 簡化場次 DTO（用於分組顯示）
/// </summary>
public class ShowtimeSimpleDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    public int TheaterId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（HH:mm）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;
}
```

---

### Service 層

#### [MODIFY] `IDailyScheduleService.cs`

新增方法：

```csharp
/// <summary>
/// 取得分組時刻表（用於側邊欄顯示）
/// </summary>
/// <param name="date">時刻表日期</param>
/// <returns>按電影和影廳類型分組的時刻表</returns>
Task<GroupedDailyScheduleResponseDto> GetGroupedDailyScheduleAsync(DateTime date);
```

#### [MODIFY] `DailyScheduleService.cs`

實作 `GetGroupedDailyScheduleAsync` 方法：

```csharp
public async Task<GroupedDailyScheduleResponseDto> GetGroupedDailyScheduleAsync(DateTime date)
{
    var scheduleDate = date.Date;

    // 1. 查詢時刻表
    var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(scheduleDate);
    if (dailySchedule == null)
    {
        throw new KeyNotFoundException($"日期 {scheduleDate:yyyy-MM-dd} 的時刻表不存在");
    }

    // 2. 取得該日期的所有場次（包含電影、影廳資訊）
    var showtimes = await _showtimeRepository.GetByDateWithDetailsAsync(scheduleDate);

    // 3. 按電影分組
    var movieGroups = showtimes
        .GroupBy(s => new { s.MovieId, s.Movie.Title, s.Movie.PosterUrl, s.Movie.Rating, s.Movie.Duration })
        .Select(movieGroup =>
        {
            // 4. 每個電影內，按影廳類型分組
            var theaterTypeGroups = movieGroup
                .GroupBy(s => s.Theater.Type)
                .Select(typeGroup =>
                {
                    var showtimesList = typeGroup
                        .Select(s => new ShowtimeSimpleDto
                        {
                            Id = s.Id,
                            TheaterId = s.TheaterId,
                            TheaterName = s.Theater.Name,
                            StartTime = s.StartTime.ToString(@"hh\:mm"),
                            EndTime = s.StartTime.Add(TimeSpan.FromMinutes(s.Movie.Duration)).ToString(@"hh\:mm")
                        })
                        .OrderBy(s => s.StartTime)
                        .ToList();

                    // 計算時間範圍
                    var minStartTime = showtimesList.Min(s => s.StartTime);
                    var maxEndTime = showtimesList.Max(s => s.EndTime);
                    var timeRange = minStartTime == maxEndTime ? minStartTime : $"{minStartTime} {maxEndTime}";

                    return new TheaterTypeGroupDto
                    {
                        TheaterType = typeGroup.Key,
                        TheaterTypeDisplay = ConvertTheaterTypeToDisplay(typeGroup.Key),
                        TimeRange = timeRange,
                        Showtimes = showtimesList
                    };
                })
                .OrderBy(t => t.TheaterType)
                .ToList();

            return new MovieShowtimeGroupDto
            {
                MovieId = movieGroup.Key.MovieId,
                MovieTitle = movieGroup.Key.Title,
                PosterUrl = movieGroup.Key.PosterUrl,
                Rating = movieGroup.Key.Rating,
                RatingDisplay = ConvertRatingToDisplay(movieGroup.Key.Rating),
                Duration = movieGroup.Key.Duration,
                DurationDisplay = FormatDuration(movieGroup.Key.Duration),
                TheaterTypeGroups = theaterTypeGroups
            };
        })
        .OrderBy(m => m.TheaterTypeGroups.Min(t => t.Showtimes.Min(s => s.StartTime)))
        .ToList();

    return new GroupedDailyScheduleResponseDto
    {
        ScheduleDate = scheduleDate,
        Status = dailySchedule.Status,
        MovieShowtimes = movieGroups
    };
}

// 輔助方法
private string ConvertRatingToDisplay(string rating)
{
    return rating switch
    {
        "G" => "0+",      // General Audiences
        "PG" => "12+",    // Parental Guidance
        "R" => "18+",     // Restricted
        _ => "0+"
    };
}

private string FormatDuration(int minutes)
{
    var hours = minutes / 60;
    var mins = minutes % 60;
    return $"{hours} 小時 {mins} 分鐘";
}

private string ConvertTheaterTypeToDisplay(string theaterType)
{
    return theaterType switch
    {
        "Digital" => "數位",
        "4DX" => "4DX",
        "IMAX" => "IMAX",
        _ => theaterType
    };
}
```

---

### Controller 層

#### [MODIFY] `DailySchedulesController.cs`

新增 API 端點：

```csharp
/// <summary>
/// /api/admin/daily-schedules/{date}/grouped 取得分組時刻表
/// </summary>
/// <remarks>
/// 取得按電影和影廳類型分組的時刻表資料，用於側邊欄顯示。
/// 
/// **分組邏輯**：
/// 1. 第一層：按電影分組
/// 2. 第二層：每部電影下，按影廳類型（Digital、4DX、IMAX）分組
/// 3. 每個影廳類型組顯示時間範圍和場次列表
/// 
/// **資料包含**：
/// - 電影海報、名稱、分級（0+/12+/18+）、片長
/// - 影廳類型中文顯示（數位/4DX/IMAX）及其時間範圍
/// - 具體場次時間
/// 
/// **範例回應**：
/// ```json
/// {
///   "scheduleDate": "2025-12-01",
///   "status": "OnSale",
///   "movieShowtimes": [
///     {
///       "movieId": 1,
///       "movieTitle": "雲深不知處",
///       "posterUrl": "https://...",
///       "rating": "G",
///       "ratingDisplay": "0+",
///       "duration": 143,
///       "durationDisplay": "2 小時 23 分鐘",
///       "theaterTypeGroups": [
///         {
///           "theaterType": "Digital",
///           "theaterTypeDisplay": "數位",
///           "timeRange": "09:00 09:45",
///           "showtimes": [
///             { "id": 1, "theaterId": 1, "theaterName": "1廳", "startTime": "09:00", "endTime": "11:23" }
///           ]
///         }
///       ]
///     }
///   ]
/// }
/// ```
/// </remarks>
/// <param name="date">時刻表日期，格式：YYYY-MM-DD</param>
/// <response code="200">查詢成功</response>
/// <response code="400">日期格式錯誤</response>
/// <response code="404">該日期沒有時刻表記錄</response>
/// <response code="401">未授權</response>
[HttpGet("{date}/grouped")]
[ProducesResponseType(typeof(GroupedDailyScheduleResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> GetGroupedDailySchedule([FromRoute] string date)
{
    try
    {
        if (!DateTime.TryParse(date, out var scheduleDate))
        {
            return BadRequest(new { message = "日期格式錯誤，必須為 YYYY-MM-DD" });
        }

        var result = await _dailyScheduleService.GetGroupedDailyScheduleAsync(scheduleDate);
        return Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
}
```

---

## 測試計畫

### 測試檔案
建立：`docs/tests/分組時刻表API/test-grouped-schedule.http`

### 測試情境

1. **成功查詢分組時刻表** - 200 OK
2. **驗證分組邏輯** - 檢查電影和影廳類型分組
3. **驗證格式化** - 分級(0+/12+/18+)、片長、影廳類型中文
4. **日期不存在** - 404 Not Found
5. **日期格式錯誤** - 400 Bad Request

---

## 資料格式對照

### 影廳類型
- **資料庫**: Digital, 4DX, IMAX（英文）
- **顯示**: 數位, 4DX, IMAX（中文）

### 電影分級
- **資料庫**: G, PG, R（英文）
- **顯示**: 0+, 12+, 18+

### 片長
- **資料庫**: 143（分鐘）
- **顯示**: 2 小時 23 分鐘

---

## 變更檔案清單

### 新增檔案
- `Models/DTOs/GroupedDailyScheduleResponseDto.cs`
- `Models/DTOs/MovieShowtimeGroupDto.cs`
- `Models/DTOs/TheaterTypeGroupDto.cs`
- `Models/DTOs/ShowtimeSimpleDto.cs`

### 修改檔案
- `Services/Interfaces/IDailyScheduleService.cs`
- `Services/Implementations/DailyScheduleService.cs`
- `Controllers/DailySchedulesController.cs`
