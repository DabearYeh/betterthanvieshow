# 取得電影可訂票日期 API

## 目標

實作 `GET /api/movies/{movieId}/available-dates` API，查詢指定電影有哪些日期有場次，且該日期的時刻表狀態為 `OnSale`（販售中）。

## 背景

根據 `訂票.feature` 的規則：
- **Rule**: 場次日期的時刻表必須為販售中狀態
- 只有 `DailySchedule.Status = "OnSale"` 的日期才能訂票
- 草稿狀態 (`Draft`) 的場次禁止訂票

這支 API 將作為訂票流程的第一步，讓使用者選擇電影後，看到該電影有哪些日期可以訂票。

## 使用者審查項目

> [!IMPORTANT]
> **API 端點路徑確認**
> 
> 建議的 API 路徑為：`GET /api/movies/{movieId}/available-dates`
> 
> 這個路徑符合 RESTful 設計，表示「取得某個電影的可訂票日期」。是否同意此設計？

> [!IMPORTANT]
> **回應格式確認**
> 
> 根據 UI 設計圖，建議的回應格式包含：
> 
> **電影資訊**：
> - `movieId`: 電影 ID
> - `title`: 電影名稱
> - `rating`: 電影分級（例如「普遍級」）
> - `duration`: 電影時長（分鐘）
> - `genre`: 電影類型（例如「劇情」）
> - `posterUrl`: 海報 URL
> - `trailerUrl`: 預告片 URL
> 
> **可訂票日期**：
> - `dates`: 日期陣列，每個日期包含：
>   - `date`: 日期字串 (YYYY-MM-DD)
>   - `dayOfWeek`: 星期幾（例如「週四」）
> 
> 這樣前端可以在同一個 API 取得電影資訊和可訂票日期，無需額外呼叫電影詳情 API。

---

## 建議變更

### Repository 層

#### [MODIFY] [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs)

新增方法：

```csharp
/// <summary>
/// 取得指定電影的所有可訂票日期（時刻表狀態為 OnSale）
/// </summary>
/// <param name="movieId">電影 ID</param>
/// <returns>可訂票的日期列表（已去重且排序）</returns>
Task<List<DateTime>> GetAvailableDatesByMovieIdAsync(int movieId);
```

---

#### [MODIFY] [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs)

實作方法：

```csharp
/// <inheritdoc />
public async Task<List<DateTime>> GetAvailableDatesByMovieIdAsync(int movieId)
{
    return await _context.MovieShowTimes
        .Where(st => st.MovieId == movieId)
        .Join(
            _context.DailySchedules,
            st => st.ShowDate.Date,
            ds => ds.ScheduleDate.Date,
            (st, ds) => new { st.ShowDate, ds.Status }
        )
        .Where(x => x.Status == "OnSale")
        .Select(x => x.ShowDate.Date)
        .Distinct()
        .OrderBy(date => date)
        .ToListAsync();
}
```

**說明**：
- 透過 `JOIN` 查詢 `DailySchedules` 表，確保只返回狀態為 `OnSale` 的日期
- 使用 `Distinct()` 去除重複日期
- 使用 `OrderBy()` 按日期升序排序

---

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：

```csharp
/// <summary>
/// 取得指定電影的可訂票日期
/// </summary>
/// <param name="movieId">電影 ID</param>
/// <returns>可訂票日期的回應 DTO</returns>
Task<MovieAvailableDatesResponseDto?> GetAvailableDatesAsync(int movieId);
```

---

#### [MODIFY] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

實作方法：

```csharp
/// <inheritdoc />
public async Task<MovieAvailableDatesResponseDto?> GetAvailableDatesAsync(int movieId)
{
    // 1. 檢查電影是否存在
    var movie = await _movieRepository.GetByIdAsync(movieId);
    if (movie == null)
    {
        return null;
    }

    // 2. 取得可訂票日期
    var availableDates = await _showtimeRepository.GetAvailableDatesByMovieIdAsync(movieId);

    // 3. 轉換為 DTO
    var dateDtos = availableDates.Select(date => new DateItemDto
    {
        Date = date.ToString("yyyy-MM-dd"),
        DayOfWeek = GetDayOfWeekInChinese(date.DayOfWeek)
    }).ToList();

    return new MovieAvailableDatesResponseDto
    {
        MovieId = movie.Id,
        Title = movie.Title,
        Rating = movie.Rating,
        Duration = movie.Duration,
        Genre = movie.Genre,
        PosterUrl = movie.PosterUrl ?? string.Empty,
        TrailerUrl = movie.TrailerUrl ?? string.Empty,
        Dates = dateDtos
    };
}

/// <summary>
/// 將 DayOfWeek 轉換為繁體中文
/// </summary>
private string GetDayOfWeekInChinese(DayOfWeek dayOfWeek)
{
    return dayOfWeek switch
    {
        DayOfWeek.Monday => "週一",
        DayOfWeek.Tuesday => "週二",
        DayOfWeek.Wednesday => "週三",
        DayOfWeek.Thursday => "週四",
        DayOfWeek.Friday => "週五",
        DayOfWeek.Saturday => "週六",
        DayOfWeek.Sunday => "週日",
        _ => ""
    };
}
```

**業務邏輯**：
1. 驗證電影是否存在（若不存在返回 `null`）
2. 從 Repository 層取得可訂票日期
3. 將日期轉換為繁體中文星期格式
4. **組裝完整電影資訊**（包含分級、時長、類型、海報、預告片）
5. 組裝回應 DTO

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增端點：

```csharp
/// <summary>
/// 取得電影的可訂票日期
/// </summary>
/// <remarks>
/// 此端點用於訂票流程的第一步：選擇日期。
/// 
/// 返回該電影有場次且時刻表狀態為 **OnSale**（販售中）的日期列表。
/// 
/// **無需授權**，任何使用者皆可存取。
/// 
/// **業務規則**：
/// - 只返回 `DailySchedule.Status = "OnSale"` 的日期
/// - 草稿狀態 (`Draft`) 的場次不會出現在列表中
/// - 日期按升序排序
/// </remarks>
/// <param name="id">電影 ID</param>
/// <response code="200">成功取得可訂票日期列表</response>
/// <response code="404">找不到指定的電影</response>
/// <response code="500">伺服器內部錯誤</response>
/// <returns>可訂票日期列表</returns>
[HttpGet("{id}/available-dates")]
[AllowAnonymous]
[ProducesResponseType(typeof(ApiResponse<MovieAvailableDatesResponseDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetAvailableDates(int id)
{
    try
    {
        var result = await _movieService.GetAvailableDatesAsync(id);
        
        if (result == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"找不到 ID 為 {id} 的電影",
                Data = null
            });
        }

        return Ok(new ApiResponse<MovieAvailableDatesResponseDto>
        {
            Success = true,
            Message = "成功取得可訂票日期",
            Data = result
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "取得電影 {MovieId} 的可訂票日期時發生錯誤", id);
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Message = "取得可訂票日期時發生錯誤",
            Data = null
        });
    }
}
```

---

### DTO 層

#### [NEW] [MovieAvailableDatesResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieAvailableDatesResponseDto.cs)

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影可訂票日期回應 DTO
/// </summary>
public class MovieAvailableDatesResponseDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級（普遍級、輔導級、限制級）
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 電影類型（多個用逗號分隔）
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 預告片 URL
    /// </summary>
    public string TrailerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 可訂票日期列表
    /// </summary>
    public List<DateItemDto> Dates { get; set; } = new();
}

/// <summary>
/// 日期項目 DTO
/// </summary>
public class DateItemDto
{
    /// <summary>
    /// 日期（格式：YYYY-MM-DD）
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 星期幾（繁體中文，例如「週四」）
    /// </summary>
    public string DayOfWeek { get; set; } = string.Empty;
}
```

---

## 驗證計畫

### HTTP 測試

#### [NEW] [get-available-dates.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/tests/get-available-dates.http)

```http
### 取得電影可訂票日期 API 測試

@baseUrl = https://localhost:7072
@movieId = 1

### 1. 成功取得可訂票日期
GET {{baseUrl}}/api/movies/{{movieId}}/available-dates
Content-Type: application/json

### 2. 電影不存在
GET {{baseUrl}}/api/movies/999999/available-dates
Content-Type: application/json

### 3. 無效的電影 ID（負數）
GET {{baseUrl}}/api/movies/-1/available-dates
Content-Type: application/json
```

### 測試場景

1. **成功取得可訂票日期**
   - 前置條件：電影存在，且有場次的日期時刻表狀態為 `OnSale`
   - 預期結果：200 OK，返回日期列表

2. **電影不存在**
   - 前置條件：使用不存在的電影 ID
   - 預期結果：404 Not Found

3. **電影存在但無可訂票日期**
   - 前置條件：電影存在，但沒有場次或所有場次的日期時刻表狀態為 `Draft`
   - 預期結果：200 OK，返回空的日期列表

---

## 依賴注入

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)

確認 `IShowtimeRepository` 已註冊（應該已存在）：

```csharp
builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
```
