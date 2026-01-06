# 快速參考 - API 排序優化

## 修改的 API
- `GET /api/admin/theaters` - 影廳按 ID 降序（新的在前）
- `GET /api/admin/movies` - 電影按 CreatedAt 降序（新的在前）

## 修改的檔案
1. `betterthanvieshow/Services/Implementations/TheaterService.cs`
   - 方法: `GetAllTheatersAsync()`
   - 修改: 添加 `.OrderByDescending(t => t.Id)`

2. `betterthanvieshow/Services/Implementations/MovieService.cs`
   - 方法: `GetAllMoviesAsync()`
   - 修改: 添加 `.OrderByDescending(m => m.CreatedAt)`

## 快速測試

### 使用 PowerShell
```powershell
# 設定 token (替換成您的 JWT token)
$token = "YOUR_JWT_TOKEN_HERE"
$headers = @{ "Authorization" = "Bearer $token" }

# 測試影廳 API
$response = Invoke-RestMethod -Uri "http://localhost:5041/api/admin/theaters" -Method Get -Headers $headers
$response.data | Select-Object id, name, type

# 測試電影 API
$response = Invoke-RestMethod -Uri "http://localhost:5041/api/admin/movies" -Method Get -Headers $headers
$response.data | Select-Object id, title, status
```

### 使用 Scalar API Docs
1. 開啟 `http://localhost:5041/scalar/v1`
2. 使用 `POST /api/auth/login` 登入取得 token
3. 在頁面頂部輸入 Bearer token
4. 測試 `GET /api/admin/theaters` 和 `GET /api/admin/movies`

## 預期結果
- ✅ ID/CreatedAt 較大的資料在列表前面
- ✅ 新增的項目顯示在第一個位置
- ✅ 回傳的 JSON 陣列順序為降序

## Branch
`feature/sort-by-creation-time`
