# 實作電影搜尋 API

本計畫將實作電影搜尋功能，讓用戶可以透過關鍵字搜尋電影。

## 背景說明

根據前端 UI 設計，用戶在搜尋框輸入關鍵字後，系統會搜尋所有相關電影並顯示結果。

**搜尋範圍**：
- **電影標題** (Title) - 唯一搜尋欄位

## API 設計

**端點路徑**：`GET /api/movies/search?keyword={關鍵字}`

**參數**：
- `keyword` (必填) - 搜尋關鍵字，至少 1 個字元

**範例**：
```
GET /api/movies/search?keyword=復仇者
GET /api/movies/search?keyword=動作
GET /api/movies/search?keyword=克里斯
```

**搜尋邏輯**：
- 不區分大小寫
- 模糊搜尋（標題包含關鍵字即可）
- 只搜尋電影標題 (Title)
- 只返回正在上映 + 即將上映的電影（已下映的不顯示）

---

## 提議的變更

### Repository 層

#### [MODIFY] [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs)

新增方法：

```csharp
/// <summary>
/// 搜尋電影（根據關鍵字搜尋標題）
/// </summary>
/// <param name="keyword">搜尋關鍵字</param>
/// <returns>符合條件的電影列表</returns>
Task<List<Movie>> SearchMoviesAsync(string keyword);
```

#### [MODIFY] [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs)

實作搜尋方法：

```csharp
public async Task<List<Movie>> SearchMoviesAsync(string keyword)
{
    var today = DateTime.Today;
    var lowerKeyword = keyword.ToLower();

    return await _context.Movies
        .Where(m => 
            // 只搜尋正在上映或即將上映的電影
            (m.ReleaseDate <= today && m.EndDate >= today) || m.ReleaseDate > today
        )
        .Where(m => 
            // 只搜尋標題
            m.Title.ToLower().Contains(lowerKeyword)
        )
        .OrderByDescending(m => m.ReleaseDate)
        .ToListAsync();
}
```

---

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：

```csharp
/// <summary>
/// 搜尋電影
/// </summary>
/// <param name="keyword">搜尋關鍵字</param>
/// <returns>搜尋結果</returns>
Task<ApiResponse<List<MovieSimpleDto>>> SearchMoviesAsync(string keyword);
```

#### [MODIFY] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

實作搜尋方法：

```csharp
public async Task<ApiResponse<List<MovieSimpleDto>>> SearchMoviesAsync(string keyword)
{
    try
    {
        _logger.LogInformation("搜尋電影: {Keyword}", keyword);

        // 驗證關鍵字
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return ApiResponse<List<MovieSimpleDto>>.FailureResponse("搜尋關鍵字不可為空");
        }

        // 搜尋電影
        var movies = await _movieRepository.SearchMoviesAsync(keyword.Trim());

        // 轉換為 DTO
        var result = movies.Select(MapToSimpleDto).ToList();

        _logger.LogInformation("搜尋電影完成，找到 {Count} 部電影", result.Count);

        return ApiResponse<List<MovieSimpleDto>>.SuccessResponse(
            result, 
            $"找到 {result.Count} 部符合的電影"
        );
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "搜尋電影時發生錯誤: {Keyword}", keyword);
        return ApiResponse<List<MovieSimpleDto>>.FailureResponse(
            $"搜尋電影時發生錯誤: {ex.Message}"
        );
    }
}
```

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增搜尋端點：

```csharp
/// <summary>
/// 搜尋電影
/// </summary>
/// <remarks>
/// 根據關鍵字搜尋電影標題（Title）。
/// 
/// 只返回正在上映或即將上映的電影。
/// 
/// **無需授權**，任何使用者皆可存取。
/// </remarks>
/// <param name="keyword">搜尋關鍵字（必填，至少 1 個字元）</param>
/// <response code="200">成功取得搜尋結果</response>
/// <response code="400">關鍵字為空或無效</response>
/// <response code="500">伺服器內部錯誤</response>
/// <returns>符合條件的電影列表</returns>
[HttpGet("~/api/movies/search")]
[AllowAnonymous]
[ProducesResponseType(typeof(ApiResponse<List<MovieSimpleDto>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<List<MovieSimpleDto>>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<List<MovieSimpleDto>>), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> SearchMovies([FromQuery] string keyword)
{
    // 驗證關鍵字
    if (string.IsNullOrWhiteSpace(keyword))
    {
        return BadRequest(ApiResponse<List<MovieSimpleDto>>.FailureResponse(
            "搜尋關鍵字不可為空"
        ));
    }

    var result = await _movieService.SearchMoviesAsync(keyword);

    if (!result.Success)
    {
        return StatusCode(500, result);
    }

    return Ok(result);
}
```

---

## 驗證計畫

### HTTP 測試

創建測試檔案：`docs/plans/GET-movie-search/tests/search-movies.http`

**測試場景：**

1. **成功搜尋電影標題**
   ```http
   GET {{baseUrl}}/api/movies/search?keyword=復仇者
   ```
   - 預期：200 OK，返回包含「復仇者」的電影

2. **搜尋部分標題**
   ```http
   GET {{baseUrl}}/api/movies/search?keyword=復仇
   ```
   - 預期：200 OK，返回標題包含「復仇」的電影

3. **空關鍵字**
   ```http
   GET {{baseUrl}}/api/movies/search?keyword=
   ```
   - 預期：400 Bad Request，錯誤訊息：「搜尋關鍵字不可為空」

4. **找不到結果**
   ```http
   GET {{baseUrl}}/api/movies/search?keyword=不存在的電影12345
   ```
   - 預期：200 OK，返回空陣列 `[]`

### 執行測試

```bash
# 確保應用程式正在運行
cd betterthanvieshow
dotnet run
```

使用 VS Code REST Client 或 Scalar UI 測試端點。
