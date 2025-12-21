# 取得所有電影 API 實作計畫

實作 `GET /api/admin/movies` 端點，讓管理員可以查看電影列表。

## 前端 UI 參考

![電影列表 UI](C:/Users/VivoBook/.gemini/antigravity/brain/feb71003-cea8-4f60-92aa-4f0b52743d87/uploaded_image_1766335319522.png)

## API 規格

- **端點**: `GET /api/admin/movies`
- **權限**: 僅限 Admin
- **功能**: 取得所有電影列表

### 回應欄位
根據 UI 設計，每筆電影需包含：
- `id` - 電影 ID
- `title` - 片名
- `posterUrl` - 海報
- `duration` - 片長（分鐘）
- `rating` - 分級
- `releaseDate` - 上映日
- `endDate` - 下映日
- `status` - 上映狀態（計算欄位：即將上映/上映中/已下映）

---

## Proposed Changes

### DTOs

#### [NEW] [MovieListItemDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieListItemDto.cs)

專為列表設計的精簡 DTO，包含 `Status` 計算欄位。

---

### Repository 層

#### [MODIFY] [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs)

新增方法：`Task<List<Movie>> GetAllAsync();`

---

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：`Task<ApiResponse<List<MovieListItemDto>>> GetAllMoviesAsync();`

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增端點：`[HttpGet] GetAllMovies()`

---

## Verification Plan

| 測試案例 | 預期結果 |
|---------|---------|
| 成功取得電影列表 | 200 OK + 電影陣列 |
| 無電影時 | 200 OK + 空陣列 |
