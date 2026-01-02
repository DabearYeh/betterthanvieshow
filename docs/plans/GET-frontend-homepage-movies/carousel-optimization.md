# 輪播圖顯示邏輯優化

## 📋 變更概述

優化輪播圖查詢邏輯，確保只顯示**正在上映**或**即將上映**的電影，排除已下映的電影。

**變更日期**：2026-01-02  
**變更類型**：功能優化  
**影響範圍**：首頁輪播圖 API (`GET /api/movies/homepage`)

---

## 🎯 變更原因

### 問題描述
原先的輪播圖查詢邏輯只檢查 `CanCarousel` 欄位，導致：
- ✅ 即將上映的電影會顯示（符合預期，用於宣傳）
- ✅ 正在上映的電影會顯示（符合預期）
- ❌ **已下映的電影也會顯示**（不符合預期）

### 舊邏輯
```csharp
// MovieRepository.GetCarouselMoviesAsync (舊版)
return await _context.Movies
    .Where(m => m.CanCarousel)  // 只檢查這一個條件
    .OrderByDescending(m => m.ReleaseDate)
    .ToListAsync();
```

### 問題影響
- 管理員若忘記取消已下映電影的輪播勾選，該電影會持續顯示在首頁
- 使用者可能點擊已下映電影，但無法訂票，造成體驗不佳

---

## ✨ 變更內容

### 新邏輯
```csharp
// MovieRepository.GetCarouselMoviesAsync (新版)
public async Task<List<Movie>> GetCarouselMoviesAsync()
{
    var today = DateTime.Today;
    return await _context.Movies
        .Where(m => m.CanCarousel && m.EndDate >= today)  // 新增日期過濾
        .OrderByDescending(m => m.ReleaseDate)
        .ToListAsync();
}
```

### 篩選條件

| 條件 | 說明 |
|------|------|
| `CanCarousel == true` | 管理員勾選「加入輪播」 |
| `EndDate >= today` | **新增**：排除已下映的電影 |

### 💡 邏輯巧妙之處

**單一條件 `EndDate >= today` 自然涵蓋兩種狀態**：

```
即將上映 (ReleaseDate > today, EndDate >= today) 
     + 
正在上映 (ReleaseDate <= today <= EndDate)
     =
未下映 (EndDate >= today)  ← 簡潔的實作！
```

無需複雜的 OR 條件判斷，因為「即將上映」和「正在上映」的**共同特徵**就是「還沒下映」。

### 顯示規則

| 電影狀態 | 條件判斷 | 舊邏輯 | 新邏輯 | 說明 |
|---------|---------|--------|--------|------|
| **即將上映** | `ReleaseDate > today` <br> `EndDate >= today` ✅ | ✅ 顯示 | ✅ 顯示 | 用於電影宣傳 |
| **正在上映** | `ReleaseDate <= today <= EndDate` <br> `EndDate >= today` ✅ | ✅ 顯示 | ✅ 顯示 | 主要內容 |
| **已下映** | `EndDate < today` <br> `EndDate >= today` ❌ | ❌ **錯誤顯示** | ✅ **正確隱藏** | 修正問題 |

---

## 📝 修改檔案清單

### 1. 核心邏輯
- ✅ `betterthanvieshow/Repositories/Implementations/MovieRepository.cs`
  - 修改 `GetCarouselMoviesAsync` 方法
  - 新增日期過濾條件：`m.EndDate >= today`

### 2. 介面定義
- ✅ `betterthanvieshow/Repositories/Interfaces/IMovieRepository.cs`
  - 更新方法註解，說明過濾邏輯

### 3. DTO 文件
- ✅ `betterthanvieshow/Models/DTOs/HomepageMoviesResponseDto.cs`
  - 更新 `Carousel` 屬性註解

### 4. 資料庫規格
- ✅ `docs/spec/erm.dbml`
  - 更新 `Movie.can_carousel` 欄位說明
  - 補充 Note 中的輪播顯示邏輯

---

## 🧪 測試建議

### 手動測試
1. **建立測試資料**
   ```
   電影 A：2025-11-01 ~ 2025-11-30（已下映），CanCarousel = true
   電影 B：2025-12-01 ~ 2026-01-31（正在上映），CanCarousel = true
   電影 C：2026-02-01 ~ 2026-02-28（即將上映），CanCarousel = true
   電影 D：2025-12-01 ~ 2026-01-31（正在上映），CanCarousel = false
   ```

2. **呼叫 API**
   ```http
   GET /api/movies/homepage
   ```

3. **預期結果**
   - `Carousel` 陣列應包含：電影 B、電影 C
   - `Carousel` 陣列**不應包含**：電影 A（已下映）、電影 D（未勾選輪播）

### SQL 驗證查詢
```sql
-- 查詢當前應顯示在輪播的電影
SELECT Id, Title, ReleaseDate, EndDate, CanCarousel
FROM Movie
WHERE CanCarousel = 1 
  AND EndDate >= CAST(GETDATE() AS DATE)
ORDER BY ReleaseDate DESC;
```

---

## 🔄 向後相容性

### API 回應格式
- ✅ **無變更**：API 回應結構保持不變
- ✅ **無破壞性**：前端不需要修改

### 資料庫結構
- ✅ **無變更**：不需要資料庫遷移
- ✅ **純查詢邏輯**：只修改 LINQ 查詢條件

### 管理員操作
- ⚠️ **行為變更**：已下映電影即使勾選 `CanCarousel`，也不會顯示在輪播
- ✅ **自動優化**：管理員不需手動取消已下映電影的輪播勾選

---

## 📊 預期效益

### 使用者體驗
- ✅ 首頁輪播只顯示可訂票的電影
- ✅ 減少無效點擊（避免點到已下映電影）

### 營運效率
- ✅ 管理員不需手動維護輪播清單
- ✅ 系統自動過濾過期內容

### 系統穩定性
- ✅ 邏輯更清晰，減少資料維護錯誤

---

## 🎉 總結

這次優化讓輪播圖的顯示邏輯更加智能化且簡潔：

### 變更對比

**變更前**：
```csharp
CanCarousel == true  // 所有勾選的電影，包含已下映
```

**變更後**：
```csharp
CanCarousel == true && EndDate >= today  // 只顯示未下映的電影
```

### 設計亮點

✨ **簡潔性**：單一條件 `EndDate >= today` 自動涵蓋「正在上映」和「即將上映」兩種狀態  
✨ **智能性**：系統自動過濾過期內容，無需人工維護  
✨ **完整性**：確保輪播圖只顯示可訂票的電影，提升使用者體驗  
✨ **相容性**：不影響 API 格式和資料庫結構，前端無感升級

此變更符合業務邏輯，提升使用者體驗，且不影響系統其他功能。
