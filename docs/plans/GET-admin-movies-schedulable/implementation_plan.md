# 實作「取得可排程電影」API

本計畫將實作一個後台專用 API，用於在編輯特定日期的時刻表時，取得該日期可供排程的電影列表（即該日期在電影的上映期間內）。

## API 命名規範

**端點路徑：`GET /api/admin/movies/schedulable`**

- **權限**：Admin only
- **用途**：後台排程介面，列出當天可用的電影來源列表。

## 背景說明

如使用者提供的[介面截圖](file:///C:/Users/VivoBook/.gemini/antigravity/brain/ac8ab272-3cd8-40ac-a280-c28c91e37993/uploaded_image_1767341520681.png)所示，在編輯單日時刻表時，左側有一個「電影」列表，顯示當天可供排程的電影。
每部電影顯示：
- 片名
- 海報
- 時長（顯示格式為 "X 小時 Y 分鐘"）
- 分級 (圖示，後端回傳代碼)

## 提議的變更

### DTO 層

#### [NEW] `SchedulableMovieDto.cs`

建立專用於此列表的 DTO：

```csharp
public class SchedulableMovieDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string PosterUrl { get; set; }
    public int Duration { get; set; } // 原始分鐘數
    public string Genre { get; set; }
}
```

### Repository 層

#### [MODIFY] `IMovieRepository.cs`

新增方法：

```csharp
// 取得指定日期處於上映期間的電影
Task<List<Movie>> GetMoviesActiveOnDateAsync(DateTime date);
```

#### [MODIFY] `MovieRepository.cs`

實作邏輯：
```csharp
public async Task<List<Movie>> GetMoviesActiveOnDateAsync(DateTime date)
{
    // 電影必須在該日期有效：ReleaseDate <= Date <= EndDate
    // date 參數通常有時間部分，需正規化為 Date 比較
    var targetDate = date.Date;
    
    return await _context.Movies
        .Where(m => m.ReleaseDate.Date <= targetDate && m.EndDate.Date >= targetDate)
        .OrderBy(m => m.ReleaseDate) // 或按 Title 排序，視需求而定
        .ToListAsync();
}
```

### Service 層

#### [MODIFY] `IMovieService.cs`

新增方法：
```csharp
Task<ApiResponse<List<SchedulableMovieDto>>> GetSchedulableMoviesAsync(DateTime date);
```

#### [MODIFY] `MovieService.cs`

實作邏輯：
1. 呼叫 `_movieRepository.GetMoviesActiveOnDateAsync(date)`
2. 轉換為 `SchedulableMovieDto`
3. 回傳

### Controller 層

#### [MODIFY] `MoviesController.cs`

新增端點：

```csharp
[HttpGet("schedulable")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetSchedulableMovies([FromQuery] DateTime date)
{
    // ...
}
```

## 驗證計畫

### HTTP 測試

建立 `docs/plans/GET-admin-movies-schedulable/tests/get-schedulable.http`：

1. **基本查詢**
   - GET `/api/admin/movies/schedulable?date=2025-12-31`
   - 預期：回傳包含該日期有效的電影列表。

2. **邊界測試**
   - 查詢日期 < 上映日期 -> 不應出現
   - 查詢日期 > 下映日期 -> 不應出現
