# 取得所有電影 API 實作完成

✅ **狀態**: 已完成並通過測試

## 測試結果

| 測試案例 | 狀態 | 結果 |
|---------|------|------|
| 取得電影列表 | ✅ | 200 OK |

---

## 變更的檔案

### 新增
- [MovieListItemDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieListItemDto.cs) - 精簡列表 DTO

### 修改
| 檔案 | 變更 |
|-----|------|
| [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs) | 新增 `GetAllAsync` |
| [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs) | 實作 `GetAllAsync` |
| [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs) | 新增 `GetAllMoviesAsync` |
| [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs) | 實作 `GetAllMoviesAsync` + `GetMovieStatus` |
| [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs) | 新增 `GET` 端點 |

---

## API 規格

| 項目 | 值 |
|-----|---|
| 端點 | `GET /api/admin/movies` |
| 權限 | Admin |
| 成功回應 | 200 OK |

### 回應欄位
- `id`, `title`, `posterUrl`, `duration`, `rating`, `releaseDate`, `endDate`
- `status` - 計算欄位：即將上映 / 上映中 / 已下映
