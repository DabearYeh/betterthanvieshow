# 實作前台首頁電影 API

本計畫將實作前台首頁所需的電影資料 API，透過單一端點 `GET /api/movies/homepage` 提供所有區塊的資料。

## API 命名規範

**端點路徑：`GET /api/movies/homepage`**

採用 RESTful API 設計原則，以**資源（Resource）**為中心：
- ✅ `/api/movies/*` - 公開的電影資源（首頁、列表、詳情等）
- ✅ `/api/admin/*` - 後台管理功能（需要 Admin 權限）
- ✅ `/api/user/*` - 使用者個人資料（訂單、個人資訊、票券等）

首頁電影資料是**公開資源**，不屬於特定使用者，因此使用 `/api/movies/homepage` 而非 `/api/user/movies/homepage`。

## 背景說明

根據前端 UI 設計，首頁包含以下 5 個電影區塊：
1. **輪播圖** - 標記為可輪播的電影（`CanCarousel = true`）
2. **本週前10** - 本週銷售最佳的 10 部電影
3. **即將上映** - 上映日期在今天之後的電影
4. **隨機推薦** - 隨機推薦的電影
5. **所有電影** - 正在上映 + 即將上映的所有電影

由於使用者表示不需要分頁功能，因此採用**單一 API** 設計，一次性返回所有資料。

> [!IMPORTANT]
> **本週前10功能限制**
> 
> 目前系統尚未實作訂單（Order）和票券（Ticket）功能，因此無法統計實際銷售數量。
> 
> **替代方案**：暫時使用「最新建立的 10 部正在上映電影」作為本週前10，待票券系統實作後再改為根據實際銷售數量排序。

---

## 提議的變更

### DTO 層

#### [NEW] [MovieSimpleDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieSimpleDto.cs)

創建簡化版的電影 DTO，用於首頁列表展示：

```csharp
public class MovieSimpleDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string PosterUrl { get; set; }
    public int Duration { get; set; }
    public string Genre { get; set; }
    public string Rating { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? DaysUntilRelease { get; set; } // [NEW] 倒數天數
}
```

---

### Repository 層

#### [MODIFY] [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs)

新增以下方法：

```csharp
// 取得輪播電影（CanCarousel = true）
Task<List<Movie>> GetCarouselMoviesAsync();

// 取得即將上映電影（ReleaseDate > 今天）
Task<List<Movie>> GetComingSoonMoviesAsync();

// 取得正在上映電影（今天在 [ReleaseDate, EndDate] 範圍內）
Task<List<Movie>> GetMoviesOnSaleAsync();

// 取得最新建立的正在上映電影（用於本週前10的暫時替代方案）
Task<List<Movie>> GetRecentOnSaleMoviesAsync(int count);
```

#### [MODIFY] [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs)

實作上述方法，使用 LINQ 查詢過濾電影：
- `GetCarouselMoviesAsync`：`Where(m => m.CanCarousel)`
- `GetComingSoonMoviesAsync`：`Where(m => m.ReleaseDate > DateTime.Today)`
- `GetMoviesOnSaleAsync`：`Where(m => m.ReleaseDate <= DateTime.Today && m.EndDate >= DateTime.Today)`
- `GetRecentOnSaleMoviesAsync`：結合正在上映條件，按 `CreatedAt` 降序，取前 N 筆

---

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：

```csharp
/// <summary>
/// 取得首頁電影資料（輪播、本週前10、即將上映、隨機推薦、所有電影）
/// </summary>
/// <returns>首頁電影資料</returns>
Task<ApiResponse<HomepageMoviesResponseDto>> GetHomepageMoviesAsync();
```

#### [MODIFY] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

實作 `GetHomepageMoviesAsync` 方法：

**邏輯流程：**
1. **輪播圖**：呼叫 `GetCarouselMoviesAsync()`
2. **本週前10**：呼叫 `GetRecentOnSaleMoviesAsync(10)`（暫時替代方案）
3. **即將上映**：呼叫 `GetComingSoonMoviesAsync()`
4. **隨機推薦**：
   - 取得所有正在上映的電影
   - 使用 `Random.Shared.Shuffle()` 隨機排序
   - 取前 10 部
5. **所有電影**：合併正在上映 + 即將上映的電影列表

將所有 `Movie` 實體轉換為 `MovieSimpleDto` 並組裝成 `HomepageMoviesResponseDto` 回傳。

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增公開端點（不需授權）：

```csharp
/// <summary>
/// 取得首頁電影資料
/// </summary>
/// <remarks>
/// 此端點提供前台首頁所需的所有電影資料，包含：
/// - 輪播圖電影（標記為可輪播的電影）
/// - 本週前10（目前為最新建立的10部正在上映電影，未來將改為根據票券銷售數量排序）
/// - 即將上映（上映日期在今天之後的電影）
/// - 隨機推薦（隨機推薦10部正在上映的電影）
/// - 所有電影（正在上映 + 即將上映的完整列表）
/// 
/// **無需授權**，任何使用者皆可存取。
/// </remarks>
/// <response code="200">成功取得首頁電影資料</response>
[HttpGet("homepage")]
[AllowAnonymous]
public async Task<IActionResult> GetHomepageMovies()
{
    var result = await _movieService.GetHomepageMoviesAsync();
    return Ok(result);
}
```

---

## 驗證計畫

### HTTP 測試

創建測試檔案：`docs/plans/GET-frontend-homepage-movies/tests/get-homepage-movies.http`

**測試場景：**

1. **成功取得首頁資料**
   ```http
   GET {{baseUrl}}/api/movies/homepage
   ```
   - 預期：200 OK
   - 驗證回應包含所有 5 個區塊：`carousel`、`topWeekly`、`comingSoon`、`recommended`、`allMovies`
   - 驗證每個電影物件包含必要欄位：`id`、`title`、`posterUrl`、`duration`、`rating`、`releaseDate`、`endDate`、`daysUntilRelease`

2. **驗證輪播電影**
   - 確認 `carousel` 區塊只包含 `CanCarousel = true` 的電影

3. **驗證即將上映**
   - 確認 `comingSoon` 區塊的電影 `releaseDate` 都在今天之後

4. **驗證所有電影**
   - 確認 `allMovies` 包含正在上映和即將上映的電影
   - 不包含已下映的電影（`endDate < 今天`）

### 執行測試指令

將測試檔案在 VS Code 的 REST Client 擴充套件中執行，或使用指令：

```bash
# 確保應用程式正在運行
cd betterthanvieshow
dotnet run

# 在另一個終端使用 curl 測試
curl -X GET "https://localhost:7xxx/api/movies/homepage"
```

### 手動驗證步驟

1. 啟動應用程式：`dotnet run`
2. 開啟 Scalar UI：`https://localhost:7xxx/scalar/v1`
3. 找到 `GET /api/movies/homepage` 端點
4. 點擊「Try it out」並執行
5. 檢查回應資料結構是否符合 `HomepageMoviesResponseDto` 格式
6. 驗證各區塊的資料邏輯是否正確
