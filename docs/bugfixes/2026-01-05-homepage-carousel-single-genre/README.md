# Homepage Carousel Genre 修改為單一類型

**日期**: 2026-01-05  
**類型**: 功能優化  
**影響範圍**: Homepage API - Carousel  
**狀態**: ✅ 已完成並測試

---

## 📋 問題描述

在 `GET /api/movies/homepage` API 的回應中，`carousel`（輪播）部分的 `genre` 欄位會返回完整的類型字串（例如：`"Comedy, Action"`），導致前端顯示時可能有排版問題。

### 原始行為

```json
{
  "carousel": [
    {
      "id": 1,
      "title": "電影標題",
      "genre": "Comedy, Action",  // 多個類型，可能太長
      ...
    }
  ]
}
```

### 需求變更

前端只需要顯示**第一個類型**即可，不需要完整的類型列表。

### 期望行為

```json
{
  "carousel": [
    {
      "id": 1,
      "title": "電影標題",
      "genre": "Comedy",  // 只顯示第一個類型
      ...
    }
  ]
}
```

**重要**: 只有 **Carousel（輪播）** 需要修改，其他列表（TopWeekly, ComingSoon, Recommended, AllMovies）保持原樣。

---

## 🎯 解決方案

在 `MovieService` 中創建專門的映射方法 `MapToCarouselDto`，只用於 Carousel，該方法會將 Genre 分割並只取第一個類型。

---

## 🔧 技術實作

### 修改檔案

**檔案位置**: `betterthanvieshow/Services/Implementations/MovieService.cs`

### 1. 新增專門的映射方法

在第 385-406 行新增 `MapToCarouselDto` 方法：

```csharp
/// <summary>
/// 將 Movie 實體轉換為 MovieSimpleDto (用於 Carousel，只返回第一個 Genre)
/// </summary>
private static MovieSimpleDto MapToCarouselDto(Movie movie)
{
    // 只取第一個類型（例如 "Comedy, Action" 變成 "Comedy"）
    var firstGenre = movie.Genre.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault() ?? movie.Genre;
    
    return new MovieSimpleDto
    {
        Id = movie.Id,
        Title = movie.Title,
        PosterUrl = movie.PosterUrl,
        Duration = movie.Duration,
        Genre = firstGenre,  // 只返回第一個類型
        Rating = movie.Rating,
        ReleaseDate = movie.ReleaseDate,
        EndDate = movie.EndDate,
        DaysUntilRelease = movie.ReleaseDate.Date > DateTime.UtcNow.Date 
            ? (movie.ReleaseDate.Date - DateTime.UtcNow.Date).Days 
            : null
    };
}
```

### 2. 修改 Carousel 使用新方法

在第 342 行，修改 Carousel 使用新的映射方法：

**修改前**:
```csharp
Carousel = carouselMovies.Select(MapToSimpleDto).ToList(),
```

**修改後**:
```csharp
Carousel = carouselMovies.Select(MapToCarouselDto).ToList(),
```

### 3. 其他列表保持不變

```csharp
var response = new HomepageMoviesResponseDto
{
    Carousel = carouselMovies.Select(MapToCarouselDto).ToList(),      // ← 使用新方法
    TopWeekly = topWeeklyMovies.Select(MapToSimpleDto).ToList(),       // ← 保持原方法
    ComingSoon = comingSoonMovies.Select(MapToSimpleDto).ToList(),     // ← 保持原方法
    Recommended = recommendedMovies.Select(MapToSimpleDto).ToList(),   // ← 保持原方法
    AllMovies = allMovies.Select(MapToSimpleDto).ToList()              // ← 保持原方法
};
```

---

## 🧪 測試結果

### API 回應範例

**請求**:
```http
GET /api/movies/homepage HTTP/1.1
```

**回應 - Carousel 部分**:
```json
{
  "success": true,
  "data": {
    "carousel": [
      {
        "id": 1,
        "title": "胎尼這次出道保證不下架",
        "genre": "Romance",  // ← 只有第一個類型
        "posterUrl": "...",
        "duration": 120,
        "rating": "PG"
      },
      {
        "id": 2,
        "title": "吉伊卡哇",
        "genre": "SciFi",  // ← 只有第一個類型
        "posterUrl": "...",
        "duration": 95,
        "rating": "G"
      }
    ],
    "topWeekly": [
      {
        "id": 3,
        "title": "範例電影",
        "genre": "Action, Adventure",  // ← 保持完整
        "posterUrl": "...",
        "duration": 110,
        "rating": "PG"
      }
    ]
  }
}
```

### 測試驗證

**測試日期**: 2026-01-05  
**測試環境**: Development (http://localhost:5041)

**測試結果**:

| 電影 | Genre | 狀態 |
|------|-------|------|
| 胎尼這次出道保證不下架 | Romance | ✅ PASS |
| 吉伊卡哇 | SciFi | ✅ PASS |
| 奇怪的知識增加了 | Adventure | ✅ PASS |
| 雲深不知夢 | Adventure | ✅ PASS |

**驗證結果**: ✅ **PASS** - 所有 Carousel 電影的 Genre 都只有一個類型

詳細測試結果請參考: [test_results.md](./test_results.md)

---

## 📊 影響分析

### 修改範圍

| 列表 | Genre 格式 | 是否修改 |
|------|-----------|---------|
| **Carousel** | 單一類型（如 "Comedy"） | ✅ 是 |
| TopWeekly | 完整類型（如 "Comedy, Action"） | ❌ 否 |
| ComingSoon | 完整類型（如 "Comedy, Action"） | ❌ 否 |
| Recommended | 完整類型（如 "Comedy, Action"） | ❌ 否 |
| AllMovies | 完整類型（如 "Comedy, Action"） | ❌ 否 |

### 處理邏輯

```csharp
// 分割 Genre 字串，取第一個元素
var firstGenre = movie.Genre.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault() ?? movie.Genre;
```

**範例**:
- 輸入: `"Comedy, Action"` → 輸出: `"Comedy"`
- 輸入: `"SciFi"` → 輸出: `"SciFi"`
- 輸入: `""` → 輸出: `""`（fallback 到原值）

---

## 🔄 向後相容性

✅ **完全向後相容**

- 只修改 Carousel 的 Genre 欄位格式
- 其他 API 端點不受影響
- 只影響回應資料格式，不影響資料庫
- 前端如果已經處理多個 Genre，現在只會收到一個，不會出錯

---

## 📱 前端整合建議

### 顯示建議

**Carousel 輪播**:
```jsx
function CarouselCard({ movie }) {
  return (
    <div className="carousel-card">
      <img src={movie.posterUrl} alt={movie.title} />
      <h3>{movie.title}</h3>
      <span className="genre-badge">{movie.genre}</span>  {/* 只有一個類型 */}
    </div>
  );
}
```

**其他列表**:
```jsx
function MovieCard({ movie }) {
  // 可能需要處理多個類型
  const genres = movie.genre.split(',').map(g => g.trim());
  
  return (
    <div className="movie-card">
      <h3>{movie.title}</h3>
      <div className="genres">
        {genres.map(genre => (
          <span key={genre} className="genre-tag">{genre}</span>
        ))}
      </div>
    </div>
  );
}
```

---

## 📝 相關檔案

### 修改的檔案
- `betterthanvieshow/Services/Implementations/MovieService.cs` (第 342 行, 第 385-406 行)

### 測試檔案
- `test_carousel_simple.ps1` - PowerShell 測試腳本

### 文件
- `README.md` - 此文件
- `test_results.md` - 詳細測試結果
- `QUICK_REFERENCE.md` - 快速參考指南

---

## ✅ 檢查清單

- [x] 新增 MapToCarouselDto 方法
- [x] 修改 Carousel 使用新方法
- [x] 確保其他列表不受影響
- [x] 編譯成功
- [x] 功能測試通過
- [x] 驗證 Carousel Genre 為單一類型
- [x] 驗證其他列表 Genre 保持完整
- [x] 建立測試腳本
- [x] 撰寫技術文件

---

## 👥 負責人

**開發者**: Gemini (AI Assistant)  
**審核者**: 待指定  
**測試者**: 待指定

---

## 📌 備註

- 此修改只影響 Homepage API 的 Carousel 部分
- Genre 在資料庫中仍然儲存完整字串
- 如果需要在其他地方使用單一 Genre，可以複用 `MapToCarouselDto` 方法
- 建議前端 Carousel 組件簡化 Genre 顯示邏輯
