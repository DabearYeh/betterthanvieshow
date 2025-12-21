# 新增電影 API 實作完成

✅ **狀態**: 已完成並通過所有測試

## 測試結果

| 測試案例 | 狀態 | 結果 |
|---------|------|------|
| 新增電影（完整欄位） | ✅ | 201 Created |
| 時長為 0 | ✅ | 400 Bad Request |
| 下映日 < 上映日 | ✅ | 400 Bad Request |

## 建立的檔案

### Entity
- [Movie.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Movie.cs) - 電影實體

### DTOs
- [CreateMovieRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CreateMovieRequestDto.cs) - 請求 DTO
- [MovieResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieResponseDto.cs) - 回應 DTO

### Repository
- [IMovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs) - 介面
- [MovieRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/MovieRepository.cs) - 實作

### Service
- [IMovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs) - 介面
- [MovieService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs) - 實作

### Controller
- [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs) - API 控制器

---

## 更新的檔案

| 檔案 | 變更 |
|-----|------|
| [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs) | 註冊 Movie DI 服務 |
| [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs) | 新增 Movies DbSet 及實體配置 |
| [betterthanvieshow.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/betterthanvieshow.http) | 新增測試案例 |

---

## Migration

```powershell
dotnet ef migrations add AddMovieEntity  # ✅ 已執行
dotnet ef database update                # ✅ 已執行
```

---

## 測試步驟

> [!IMPORTANT]
> 測試需要 **Admin 角色**的 JWT Token。

### 1. 取得 Admin Token

由於系統預設註冊為 Customer，需手動將使用者角色改為 Admin：

```sql
UPDATE [User] SET Role = 'Admin' WHERE Email = 'test@example.com';
```

或註冊新帳號後執行上述 SQL。

### 2. 登入取得 Token

```http
POST http://localhost:5041/api/Auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "SecurePass123"
}
```

### 3. 測試新增電影

將取得的 token 填入 `betterthanvieshow.http` 中的 `@token` 變數，然後執行測試。

---

## API 規格

| 項目 | 值 |
|-----|---|
| 端點 | `POST /api/admin/movies` |
| 權限 | Admin |
| 成功回應 | 201 Created |

### 必填欄位
全部欄位皆為必填（電影一旦建立無法刪除）。
