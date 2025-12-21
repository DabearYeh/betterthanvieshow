# 編輯電影 API 實作完成

✅ **狀態**: 已完成並通過所有測試

## 測試結果

| 測試案例 | 狀態 | 結果 |
|---------|------|------|
| 成功編輯電影 | ✅ | 200 OK |
| 電影 ID 不存在 | ✅ | 404 Not Found |

---

## 變更的檔案

### 新增
- [UpdateMovieRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/UpdateMovieRequestDto.cs)

### 修改
| 檔案 | 變更 |
|-----|------|
| [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs) | 新增 `GetByIdAsync`, `UpdateAsync` |
| [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs) | 實作 `GetByIdAsync`, `UpdateAsync` |
| [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs) | 新增 `UpdateMovieAsync` |
| [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs) | 實作 `UpdateMovieAsync` |
| [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs) | 新增 `PUT /{id}` 端點 |

---

## API 規格

| 項目 | 值 |
|-----|---|
| 端點 | `PUT /api/admin/movies/{id}` |
| 權限 | Admin |
| 成功回應 | 200 OK |
| 404 | 電影不存在 |
| 400 | 驗證錯誤 |
