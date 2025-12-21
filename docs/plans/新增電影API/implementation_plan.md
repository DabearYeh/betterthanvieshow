# 新增電影 API 實作計畫

實作 `POST /api/admin/movies` 端點，讓管理員可以建立新電影。

## 前端 UI 參考

![新增電影 UI](C:/Users/VivoBook/.gemini/antigravity/brain/feb71003-cea8-4f60-92aa-4f0b52743d87/uploaded_image_1766326852707.png)

## API 規格

- **端點**: `POST /api/admin/movies`
- **權限**: 僅限 Admin
- **功能**: 建立新電影記錄

### 請求格式
```json
{
  "title": "電影名稱",
  "genre": "動作,科幻",
  "duration": 120,
  "rating": "普遍級",
  "director": "導演名稱",
  "cast": "演員A、演員B、演員C",
  "description": "電影描述",
  "trailerUrl": "https://www.youtube.com/watch?v=XXXX",
  "posterUrl": "https://example.com/poster.jpg",
  "releaseDate": "2025-12-30",
  "endDate": "2026-03-30"
}
```

### 欄位說明

> [!IMPORTANT]
> 所有欄位皆為**必填**，因為電影一旦建立就**無法刪除**。

| 欄位 | 說明 | 驗證規則 |
|-----|------|---------|
| `title` | 片名 | Required, MaxLength(200) |
| `genre` | 影片類型 | Required（多選用逗號分隔）|
| `duration` | 時長 | Required, Range(1, int.MaxValue) |
| `rating` | 分級 | Required（普遍級/輔導級/限制級）|
| `director` | 導演 | Required |
| `cast` | 演員 | Required |
| `description` | 簡介 | Required |
| `trailerUrl` | 預告片連結 | Required, Url 格式 |
| `posterUrl` | 海報 URL | Required, Url 格式 |
| `releaseDate` | 上映日 | Required |
| `endDate` | 下映日 | Required, 必須 >= releaseDate |

---

## Proposed Changes

### Models 層

#### [NEW] [Movie.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Movie.cs)

建立 Movie 實體類別：
```csharp
public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Duration { get; set; }
    public string? Genre { get; set; }
    public string? Rating { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool CanCarousel { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

#### [NEW] [CreateMovieRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CreateMovieRequestDto.cs)

建立請求 DTO，使用 DataAnnotations 驗證，**所有欄位皆為必填**：
- `Title`: Required, MaxLength(200)
- `Description`: Required
- `Duration`: Required, Range(1, int.MaxValue)
- `Genre`: Required
- `Rating`: Required
- `Director`: Required
- `Cast`: Required
- `TrailerUrl`: Required, Url 格式
- `PosterUrl`: Required, Url 格式
- `ReleaseDate`: Required
- `EndDate`: Required

---

#### [NEW] [MovieResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieResponseDto.cs)

建立回應 DTO，包含電影所有欄位。

---

### Repository 層

#### [NEW] [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs)

定義介面：
```csharp
public interface IMovieRepository
{
    Task<Movie> CreateAsync(Movie movie);
}
```

---

#### [NEW] [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs)

實作資料存取邏輯。

---

### Service 層

#### [NEW] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

定義介面：
```csharp
public interface IMovieService
{
    Task<ApiResponse<MovieResponseDto>> CreateMovieAsync(CreateMovieRequestDto request);
}
```

---

#### [NEW] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

實作業務邏輯：
- 驗證 `endDate >= releaseDate`（若兩者皆有值）
- 將 DTO 轉換為 Entity
- 呼叫 Repository 建立電影
- 回傳建立結果

---

### Controller 層

#### [NEW] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

建立 API 控制器：
- Route: `api/admin/[controller]`
- 使用 `[Authorize(Roles = "Admin")]`
- `POST`: 建立電影，回傳 `201 Created`

---

### 設定層

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)

註冊新服務：
```csharp
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
```

---

#### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)

新增 Movies DbSet：
```csharp
public DbSet<Movie> Movies { get; set; }
```

---

## Verification Plan

### HTTP 測試

使用專案的 `betterthanvieshow.http` 檔案進行測試：

**測試步驟**：
1. 啟動應用程式：`dotnet run`
2. 先執行登入取得 Admin JWT Token
3. 執行以下測試案例

| 測試案例 | 預期結果 |
|---------|---------|
| 成功建立電影（完整欄位） | 201 Created + 電影資料 |
| 成功建立電影（僅必填欄位） | 201 Created |
| 缺少片名 | 400 Bad Request + 驗證錯誤 |
| 時長為 0 | 400 Bad Request + 驗證錯誤 |
| endDate < releaseDate | 400 Bad Request + 日期錯誤 |
| 未授權（無 Token） | 401 Unauthorized |
| 非 Admin 角色 | 403 Forbidden |

---

## Migration 指令

完成程式碼後執行：
```powershell
dotnet ef migrations add AddMovieEntity
dotnet ef database update
```
