# 取得單一電影詳情 API 實作計畫

實作 `GET /api/admin/movies/{id}` 端點，讓管理員可以查看電影詳情。

## 前端 UI 參考

![電影搜尋 UI](C:/Users/VivoBook/.gemini/antigravity/brain/feb71003-cea8-4f60-92aa-4f0b52743d87/uploaded_image_1766336170551.png)

## API 規格

- **端點**: `GET /api/admin/movies/{id}`
- **權限**: 僅限 Admin
- **功能**: 取得指定 ID 的電影詳情

---

## Proposed Changes

### Service 層

#### [MODIFY] [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs)

新增方法：`Task<ApiResponse<MovieResponseDto>> GetMovieByIdAsync(int id);`

---

#### [MODIFY] [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)

實作 `GetMovieByIdAsync`，使用現有的 `_movieRepository.GetByIdAsync(id)`。

---

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

新增端點：`[HttpGet("{id}")] GetMovieById(int id)`

---

## Verification Plan

| 測試案例 | 預期結果 |
|---------|---------|
| 成功取得電影詳情 | 200 OK + 電影資料 |
| 電影 ID 不存在 | 404 Not Found |
