# 取得特定日期場次列表 API

## 目標

實作 `GET /api/movies/{movieId}/showtimes?date={date}` API，查詢指定電影在特定日期的所有場次資訊，包含影廳類型、放映時間、票價、可用座位數等。

## 背景

根據 `瀏覽場次.feature` 的規則：
- **Rule**: 只顯示販售中狀態的場次（`DailySchedule.Status = "OnSale"`）
- **Rule**: 場次須包含影廳資訊（名稱、類型）
- **Rule**: 場次須包含放映時間（日期、開始時間、結束時間）
- **Rule**: 場次須包含可用座位數（總座位數 - 已售出票數）
- **Rule**: 票價根據影廳類型決定（一般數位 300元、4DX 380元、IMAX 380元）

這支 API 將作為訂票流程的第二步，讓使用者選擇日期後，查看該電影在該日期有哪些場次可以訂票。

## 使用者審查項目

> [!IMPORTANT]
> **API 端點路徑確認**
> 
> 建議的 API 路徑為：`GET /api/movies/{movieId}/showtimes?date={date}`
> 
> - `{movieId}`: 電影 ID
> - `date`: 查詢參數，格式 `YYYY-MM-DD`
> 
> 這個路徑符合 RESTful 設計。是否同意此設計？

> [!IMPORTANT]
> **回應格式確認**
> 
> 建議的回應格式包含：
> 
> **電影資訊**：
> - `movieId`: 電影 ID
> - `movieTitle`: 電影名稱
> - `date`: 查詢的日期
> 
> **場次列表**：
> - `showtimes`: 場次陣列，每個場次包含：
>   - `showTimeId`: 場次 ID
>   - `theaterName`: 影廳名稱
>   - `theaterType`: 影廳類型（一般數位、4DX、IMAX）
>   - `startTime`: 開始時間（HH:mm）
>   - `endTime`: 結束時間（HH:mm，動態計算）
>   - `price`: 票價（根據影廳類型）
>   - `availableSeats`: 可用座位數
>   - `totalSeats`: 總座位數
> 
> 這樣前端可以在同一個 API 取得完整的場次資訊，無需額外呼叫其他 API。

---

## 建議變更

### Repository 層

#### [MODIFY] [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs)

新增方法：

```csharp
/// <summary>
/// 取得指定電影在特定日期的所有場次（包含影廳和電影資訊，且時刻表狀態為 OnSale）
/// </summary>
/// <param name="movieId">電影 ID</param>
/// <param name="date">日期</param>
/// <returns>場次列表</returns>
Task<List<MovieShowTime>> GetShowtimesByMovieAndDateAsync(int movieId, DateTime date);
```

---

#### [MODIFY] [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs)

實作方法：

```csharp
/// <inheritdoc />
public async Task<List<MovieShowTime>> GetShowtimesByMovieAndDateAsync(int movieId, DateTime date)
{
    return await _context.MovieShowTimes
        .Include(st => st.Movie)
        .Include(st => st.Theater)
        .Where(st => st.MovieId == movieId && st.ShowDate.Date == date.Date)
        .Join(
            _context.DailySchedules,
            st => st.ShowDate.Date,
            ds => ds.ScheduleDate.Date,
            (st, ds) => new { ShowTime = st, ds.Status }
        )
        .Where(x => x.Status == "OnSale")
        .Select(x => x.ShowTime)
        .OrderBy(st => st.StartTime)
        .ToListAsync();
}
```

**說明**：
- 透過 `JOIN` 查詢 `DailySchedules` 表，確保只返回狀態為 `OnSale` 的場次
- 使用 `Include` 載入關聯的 `Movie` 和 `Theater` 資料
- 按開始時間升序排序

---

#### [NEW] [ITicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketRepository.cs)

建立新的 Repository 介面用於查詢票券：

```csharp
using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 票券 Repository 介面
/// </summary>
public interface ITicketRepository
{
    /// <summary>
    /// 取得指定場次已售出的票券數量（狀態為 待支付、未使用、已使用）
    /// </summary>
    /// <param name="showTimeId">場次 ID</param>
    /// <returns>已售出票券數</returns>
    Task<int> GetSoldTicketCountByShowTimeAsync(int showTimeId);
}
```

---

#### [NEW] [TicketRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketRepository.cs)

實作 Repository：

```csharp
using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 票券 Repository 實作
/// </summary>
public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> GetSoldTicketCountByShowTimeAsync(int showTimeId)
    {
        // 只計算有效票券：待支付、未使用、已使用
        // 已過期的票券不計入
        return await _context.Tickets
            .Where(t => t.ShowTimeId == showTimeId && 
                       (t.Status == "待支付" || t.Status == "未使用" || t.Status == "已使用"))
            .CountAsync();
    }
}
```

---

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：

```csharp
/// <summary>
/// 取得指定電影在特定日期的場次列表
/// </summary>
/// <param name="movieId">電影 ID</param>
/// <param name="date">日期</param>
/// <returns>場次列表的回應 DTO</returns>
Task<MovieShowtimesResponseDto?> GetShowtimesByDateAsync(int movieId, DateTime date);
```

---

#### [MODIFY] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

注入 `ITicketRepository` 並實作方法：

```csharp
// 在建構函式注入
private readonly ITicketRepository _ticketRepository;

public MovieService(
    IMovieRepository movieRepository,
    IShowtimeRepository showtimeRepository,
    ITicketRepository ticketRepository,
    ILogger<MovieService> logger)
{
    _movieRepository = movieRepository;
    _showtimeRepository = showtimeRepository;
    _ticketRepository = ticketRepository;
    _logger = logger;
}

/// <inheritdoc />
public async Task<MovieShowtimesResponseDto?> GetShowtimesByDateAsync(int movieId, DateTime date)
{
    try
    {
        _logger.LogInformation("開始取得電影 {MovieId} 在 {Date} 的場次列表", movieId, date);

        // 1. 檢查電影是否存在
        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie == null)
        {
            _logger.LogWarning("找不到電影: ID={MovieId}", movieId);
            return null;
        }

        // 2. 取得該日期的場次（只包含 OnSale 狀態）
        var showtimes = await _showtimeRepository.GetShowtimesByMovieAndDateAsync(movieId, date);

        // 3. 為每個場次計算可用座位數
        var showtimeDtos = new List<ShowtimeItemDto>();
        foreach (var showtime in showtimes)
        {
            // 計算結束時間
            var endTime = showtime.StartTime.Add(TimeSpan.FromMinutes(showtime.Movie.Duration));

            // 查詢已售出票券數
            var soldTickets = await _ticketRepository.GetSoldTicketCountByShowTimeAsync(showtime.Id);

            // 可用座位數 = 總座位數 - 已售出票券數
            var availableSeats = showtime.Theater.TotalSeats - soldTickets;

            // 根據影廳類型決定票價
            var price = GetPriceByTheaterType(showtime.Theater.Type);

            showtimeDtos.Add(new ShowtimeItemDto
            {
                ShowTimeId = showtime.Id,
                TheaterName = showtime.Theater.Name,
                TheaterType = showtime.Theater.Type,
                StartTime = showtime.StartTime.ToString(@"hh\:mm"),
                EndTime = endTime.ToString(@"hh\:mm"),
                Price = price,
                AvailableSeats = availableSeats,
                TotalSeats = showtime.Theater.TotalSeats
            });
        }

        _logger.LogInformation("成功取得電影 {MovieId} 在 {Date} 的 {Count} 個場次", 
            movieId, date, showtimeDtos.Count);

        return new MovieShowtimesResponseDto
        {
            MovieId = movie.Id,
            MovieTitle = movie.Title,
            Date = date.ToString("yyyy-MM-dd"),
            Showtimes = showtimeDtos
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "取得電影 {MovieId} 在 {Date} 的場次列表時發生錯誤", movieId, date);
        throw;
    }
}

/// <summary>
/// 根據影廳類型取得票價
/// </summary>
private static int GetPriceByTheaterType(string theaterType)
{
    return theaterType switch
    {
        "一般數位" => 300,
        "4DX" => 380,
        "IMAX" => 380,
        _ => 300 // 預設價格
    };
}
```

**業務邏輯**：
1. 驗證電影是否存在
2. 查詢該電影在指定日期的場次（只包含 `OnSale` 狀態）
3. 為每個場次計算：
   - 結束時間（開始時間 + 電影時長）
   - 已售出票券數
   - 可用座位數
   - 票價（根據影廳類型）
4. 組裝回應 DTO

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增端點：

```csharp
/// <summary>
/// 取得電影在特定日期的場次列表
/// </summary>
/// <remarks>
/// 此端點用於訂票流程的第二步：選擇場次。
/// 
/// 返回該電影在指定日期的所有場次資訊，包含影廳、時間、票價、可用座位數等。
/// 
/// **無需授權**，任何使用者皆可存取。
/// 
/// **業務規則**：
/// - 只返回 `DailySchedule.Status = "OnSale"` 的場次
/// - 草稿狀態 (`Draft`) 的場次不會出現在列表中
/// - 場次按開始時間升序排序
/// - 結束時間由系統動態計算（開始時間 + 電影時長）
/// - 可用座位數 = 總座位數 - 已售出票券數（待支付、未使用、已使用狀態）
/// - 票價根據影廳類型決定（一般數位 300元、4DX 380元、IMAX 380元）
/// 
/// **回應資料包含**：
/// - 電影基本資訊（ID、名稱）
/// - 查詢日期
/// - 場次列表（影廳、時間、票價、座位資訊）
/// </remarks>
/// <param name="id">電影 ID</param>
/// <param name="date">日期（格式：YYYY-MM-DD）</param>
/// <response code="200">成功取得場次列表</response>
/// <response code="400">日期格式無效</response>
/// <response code="404">找不到指定的電影</response>
/// <response code="500">伺服器內部錯誤</response>
/// <returns>場次列表</returns>
[HttpGet("~/api/movies/{id}/showtimes")]
[AllowAnonymous]
[ProducesResponseType(typeof(ApiResponse<MovieShowtimesResponseDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetShowtimesByDate(int id, [FromQuery] string date)
{
    try
    {
        // 驗證日期格式
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, 
            System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "日期格式無效，請使用 YYYY-MM-DD 格式",
                Data = null
            });
        }

        var result = await _movieService.GetShowtimesByDateAsync(id, parsedDate);

        if (result == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = $"找不到 ID 為 {id} 的電影",
                Data = null
            });
        }

        return Ok(new ApiResponse<MovieShowtimesResponseDto>
        {
            Success = true,
            Message = "成功取得場次列表",
            Data = result
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "取得電影 {MovieId} 在 {Date} 的場次列表時發生錯誤", id, date);
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Message = "取得場次列表時發生錯誤",
            Data = null
        });
    }
}
```

---

### DTO 層

#### [NEW] [MovieShowtimesResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieShowtimesResponseDto.cs)

```csharp
namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影場次列表回應 DTO
/// </summary>
public class MovieShowtimesResponseDto
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
    /// 查詢日期（格式：YYYY-MM-DD）
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 場次列表
    /// </summary>
    public List<ShowtimeItemDto> Showtimes { get; set; } = new();
}

/// <summary>
/// 場次項目 DTO
/// </summary>
public class ShowtimeItemDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型（一般數位、4DX、IMAX）
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（格式：HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（格式：HH:mm，動態計算）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 票價（根據影廳類型）
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 可用座位數
    /// </summary>
    public int AvailableSeats { get; set; }

    /// <summary>
    /// 總座位數
    /// </summary>
    public int TotalSeats { get; set; }
}
```

---

### Entity 層

#### [MODIFY] [Ticket.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Ticket.cs)

確認 Ticket 實體包含必要的屬性（若尚未建立則需建立）。

---

## 驗證計畫

### HTTP 測試

#### [NEW] [get-showtimes-by-date.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/訂票API-選擇場次/tests/get-showtimes-by-date.http)

```http
@baseUrl = http://localhost:5041

### 測試 1: 成功取得場次列表
# @name getShowtimes
# 前置條件：電影 ID 2 存在，2025-12-31 有場次且狀態為 OnSale
GET {{baseUrl}}/api/movies/2/showtimes?date=2025-12-31
Content-Type: application/json

###

### 測試 2: 電影不存在（應該返回 404）
# @name getShowtimesNotFound
GET {{baseUrl}}/api/movies/999999/showtimes?date=2025-12-31
Content-Type: application/json

###

### 測試 3: 日期格式無效（應該返回 400）
# @name getShowtimesInvalidDate
GET {{baseUrl}}/api/movies/2/showtimes?date=2025/12/31
Content-Type: application/json

###

### 測試 4: 該日期無場次或無 OnSale 狀態
# @name getShowtimesEmptyDate
GET {{baseUrl}}/api/movies/2/showtimes?date=2025-01-01
Content-Type: application/json

###
```

### 測試場景

1. **成功取得場次列表**
   - 前置條件：電影存在，指定日期有場次且時刻表狀態為 `OnSale`
   - 預期結果：200 OK，返回場次列表包含所有必要資訊

2. **電影不存在**
   - 前置條件：使用不存在的電影 ID
   - 預期結果：404 Not Found

3. **日期格式無效**
   - 前置條件：使用錯誤的日期格式
   - 預期結果：400 Bad Request

4. **該日期無場次**
   - 前置條件：電影存在，但指定日期無場次或狀態為 `Draft`
   - 預期結果：200 OK，返回空的場次列表

---

## 依賴注入

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)

新增 `ITicketRepository` 註冊：

```csharp
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
```
