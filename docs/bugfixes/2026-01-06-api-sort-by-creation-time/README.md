# API 排序方式優化 - 依創建時間排序

## 修改日期
2026-01-06

## 問題描述
前端需要修改影廳和電影列表的排序方式，讓新增的資料顯示在最前面，方便管理員快速查看最新建立的項目。

### 原始行為
- `GET /api/admin/theaters` - 影廳列表沒有特定排序，順序不確定
- `GET /api/admin/movies` - 電影列表沒有特定排序，順序不確定

### 期望行為
- `GET /api/admin/theaters` - 影廳按創建順序降序排列（新增的在最前面）
- `GET /api/admin/movies` - 電影按創建時間降序排列（新增的在最前面）

## 技術實作

### 影響的 API 端點
1. `GET /api/admin/theaters` - 取得所有影廳
2. `GET /api/admin/movies` - 取得所有電影

### 修改的檔案

#### 1. TheaterService.cs
**路徑**: `betterthanvieshow/Services/Implementations/TheaterService.cs`

**修改內容**:
```csharp
// 在 GetAllTheatersAsync 方法中添加排序
// 依照 ID 降序排列，新增的影廳會在最前面
foreach (var t in theaters.OrderByDescending(t => t.Id))
```

**說明**:
- 由於 `Theater` 實體沒有 `CreatedAt` 欄位，使用自增 ID 作為替代方案
- ID 較大表示較晚創建，因此按 ID 降序排列可以達到相同效果

#### 2. MovieService.cs
**路徑**: `betterthanvieshow/Services/Implementations/MovieService.cs`

**修改內容**:
```csharp
// 在 GetAllMoviesAsync 方法中添加排序
// 依照創建時間降序排列，新增的電影會在最前面
var movieList = movies
    .OrderByDescending(m => m.CreatedAt)
    .Select(m => new MovieListItemDto
    {
        // ... 映射邏輯
    }).ToList();
```

**說明**:
- `Movie` 實體有 `CreatedAt` 欄位，直接使用該欄位進行降序排序
- 確保最新創建的電影顯示在列表最前面

## 測試結果

### 測試環境
- API URL: `http://localhost:5041`
- 測試日期: 2026-01-06
- 測試工具: PowerShell + Invoke-RestMethod

### 影廳 API 測試結果
**端點**: `GET /api/admin/theaters`

**測試數據** (7 個影廳):
```
ID: 35 - 阿凡達專屬影題 (IMAX) ⬅️ 最新
ID: 33 - 龍廳 (IMAX)
ID: 32 - 測試影廳 (4DX)
ID: 31 - 殺手閣 (Digital)
ID: 14 - 大熊text廳 (IMAX)
ID: 13 - IMAX廳 (IMAX)
ID: 2 - IMAX 3D Theatre (IMAX) ⬅️ 最舊
```

✅ **結果**: ID 按降序排列，新增的影廳顯示在最前面

### 電影 API 測試結果
**端點**: `GET /api/admin/movies`

**測試數據** (9 部電影):
```
ID: 18 - 阿凡達3 (NowShowing) ⬅️ 最新
ID: 17 - racecarporche555 (NowShowing)
ID: 16 - 異種族評鑑指南解說 (ComingSoon)
ID: 15 - 吉伊卡哇 (NowShowing)
ID: 14 - 胎尼這次出道保證不下架 (ComingSoon)
ID: 13 - 奇怪的知識增加了 (NowShowing)
ID: 12 - 雲深不知夢 (NowShowing)
ID: 11 - 仙逆 (ComingSoon)
ID: 10 - 誅仙 (NowShowing) ⬅️ 較舊
```

✅ **結果**: 電影按 CreatedAt 降序排列，新增的電影顯示在最前面

## 影響範圍

### 影響的功能
- 管理後台的影廳列表顯示順序
- 管理後台的電影列表顯示順序

### 不影響的功能
- 前台使用者介面（front-end 端點不受影響）
- 其他 API 端點的排序邏輯
- 資料庫結構（無需 migration）

## 與前端的對接

前端在使用這兩個 API 時，現在會收到：
1. **影廳列表**: 最新創建的影廳在陣列的第一個位置
2. **電影列表**: 最新創建的電影在陣列的第一個位置

前端無需修改任何程式碼，直接使用回傳的順序即可。

## 後續維護建議

### Theater 實體優化建議
考慮在未來為 `Theater` 實體添加 `CreatedAt` 欄位：
1. 更符合資料庫設計最佳實踐
2. 提供更精確的創建時間記錄
3. 避免依賴自增 ID 的假設

### 實作步驟（可選）
```csharp
// 在 Theater.cs 中添加
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

然後創建 migration 並更新排序邏輯。

## 版本控制

**Branch**: `feature/sort-by-creation-time`

### 提交紀錄
```bash
git checkout -b feature/sort-by-creation-time
git add betterthanvieshow/Services/Implementations/TheaterService.cs
git add betterthanvieshow/Services/Implementations/MovieService.cs
git commit -m "feat: 修改影廳和電影 API 排序方式為依創建時間降序"
```

## 相關文件
- [快速測試腳本](./quick-test.ps1)
- [完整測試腳本](./test-api-sorting.ps1)
- [快速參考](./QUICK_REFERENCE.md)
