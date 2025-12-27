# 取得電影可訂票日期 API - 實作完成

## 📋 實作摘要

成功實作第一支訂票 API：`GET /api/movies/{movieId}/available-dates`

此 API 用於訂票流程的第一步，讓使用者選擇電影後查看該電影有哪些日期可以訂票（時刻表狀態為 `OnSale`）。

---

## ✅ 完成項目

### 1. Repository 層
- ✅ 在 [`IShowtimeRepository.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs#L48-L53) 新增 `GetAvailableDatesByMovieIdAsync` 方法
- ✅ 在 [`ShowtimeRepository.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs#L114-L128) 實作方法
  - 透過 `JOIN` 查詢 `DailySchedules` 表，確保只返回狀態為 `OnSale` 的日期
  - 使用 `Distinct()` 去除重複日期
  - 使用 `OrderBy()` 按日期升序排序

### 2. DTO 層
- ✅ 建立 [`MovieAvailableDatesResponseDto.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieAvailableDatesResponseDto.cs)
  - 包含電影完整資訊（ID、名稱、分級、時長、類型、海報、預告片）
  - 包含可訂票日期列表（日期和星期幾）

### 3. Service 層
- ✅ 在 [`IMovieService.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs#L51-L56) 新增 `GetAvailableDatesAsync` 方法
- ✅ 在 [`MovieService.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs#L386-L456) 實作方法
  - 注入 `IShowtimeRepository` 依賴
  - 驗證電影是否存在
  - 查詢可訂票日期
  - 將星期轉換為繁體中文
  - 組裝完整電影資訊和日期列表

### 4. Controller 層
- ✅ 在 [`MoviesController.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs#L138-L206) 新增 `GetAvailableDates` 端點
  - 路由：`GET /api/movies/{id}/available-dates`
  - 無需授權（`[AllowAnonymous]`）
  - 完整的 XML 文件註解
  - 錯誤處理（404、500）

### 5. HTTP 測試
- ✅ 建立 [`get-available-dates.http`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/訂票API-選擇日期/tests/get-available-dates.http) 測試檔案
  - 測試成功取得可訂票日期
  - 測試電影不存在的情況
  - 測試無效 ID
  - 測試電影存在但無可訂票日期

---

## 🏗️ 技術實作細節

### Repository 層查詢邏輯

```csharp
public async Task<List<DateTime>> GetAvailableDatesByMovieIdAsync(int movieId)
{
    return await _context.MovieShowTimes
        .Where(st => st.MovieId == movieId)
        .Join(
            _context.DailySchedules,
            st => st.ShowDate.Date,
            ds => ds.ScheduleDate.Date,
            (st, ds) => new { st.ShowDate, ds.Status }
        )
        .Where(x => x.Status == "OnSale")
        .Select(x => x.ShowDate.Date)
        .Distinct()
        .OrderBy(date => date)
        .ToListAsync();
}
```

**關鍵要點**：
- 使用 `JOIN` 確保只返回 `DailySchedule.Status = "OnSale"` 的日期
- `Distinct()` 去除同一天有多個場次的重複日期
- `OrderBy()` 按日期升序排序

### Service 層業務邏輯

1. **驗證電影存在性**：若電影不存在返回 `null`
2. **查詢可訂票日期**：呼叫 Repository 層方法
3. **轉換星期格式**：使用 `GetDayOfWeekInChinese` 輔助方法
4. **組裝完整資訊**：返回電影資訊 + 日期列表

### API 回應格式

```json
{
  "success": true,
  "message": "成功取得可訂票日期",
  "data": {
    "movieId": 1,
    "title": "黑豹",
    "rating": "普遍級",
    "duration": 134,
    "genre": "動作,科幻",
    "posterUrl": "https://...",
    "trailerUrl": "https://...",
    "dates": [
      {
        "date": "2025-12-05",
        "dayOfWeek": "週四"
      },
      {
        "date": "2025-12-06",
        "dayOfWeek": "週五"
      }
    ]
  }
}
```

---

## 🔍 驗證結果

### 編譯狀態
✅ **成功編譯**

```
betterthanvieshow net9.0 成功 (4.6 秒)
在 5.9 秒內建置 成功
```

### 應用程式啟動
✅ **成功啟動**

```
Now listening on: http://localhost:5041
```

---

## 📝 業務規則實作

根據 [`訂票.feature`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/訂票.feature#L4-L15) 的規則：

> [!NOTE]
> **場次日期的時刻表必須為販售中狀態**
> 
> - ✅ 只返回 `DailySchedule.Status = "OnSale"` 的日期
> - ✅ 草稿狀態 (`Draft`) 的場次不會出現在列表中
> - ✅ 日期按升序排序

---

## 📌 下一步

第一支 API 已完成！可以繼續開發：

1. 第二支 API：`GET /api/movies/{movieId}/showtimes?date={date}` - 取得特定日期的場次列表
2. 第三支 API：`GET /api/showtimes/{showTimeId}/seats` - 取得場次的座位配置
3. 第四支 API：`POST /api/orders` - 建立訂單（訂票）

---

## 🧪 實際測試結果

### 測試執行摘要

已完成 API 的實際測試驗證，所有測試場景通過 ✅

#### 測試 1: 成功取得可訂票日期（電影 ID: 2）

**請求**：`GET /api/movies/2/available-dates`

**回應**：
```json
{
  "success": true,
  "message": "成功取得可訂票日期",
  "data": {
    "movieId": 2,
    "title": "復仇者聯盟",
    "rating": "普遍級",
    "duration": 181,
    "genre": "動作,科幻",
    "posterUrl": "https://example.com/poster.jpg",
    "trailerUrl": "https://www.youtube.com/watch?v=test",
    "dates": [
      {
        "date": "2025-12-31",
        "dayOfWeek": "週三"
      }
    ]
  },
  "errors": null
}
```

**驗證結果**：
- ✅ HTTP 200 OK
- ✅ 返回完整電影資訊（名稱、分級、時長、類型、海報、預告片）
- ✅ 返回可訂票日期列表
- ✅ 日期格式正確（`YYYY-MM-DD`）
- ✅ 星期為繁體中文（`週三`）
- ✅ 只返回狀態為 `OnSale` 的日期

---

#### 測試 2: 電影存在但無可訂票日期（電影 ID: 1）

**請求**：`GET /api/movies/1/available-dates`

**回應**：
```json
{
  "success": true,
  "message": "成功取得可訂票日期",
  "data": {
    "movieId": 1,
    "title": "復仇者聯盟 - 已編輯",
    "rating": "輔導級",
    "duration": 200,
    "genre": "動作,科幻,冒險",
    "posterUrl": "https://example.com/poster-new.jpg",
    "trailerUrl": "https://www.youtube.com/watch?v=updated",
    "dates": []
  },
  "errors": null
}
```

**驗證結果**：
- ✅ HTTP 200 OK
- ✅ 返回完整電影資訊
- ✅ `dates` 為空陣列（該電影無 `OnSale` 狀態的場次）

---

#### 測試 3: 電影不存在（電影 ID: 999999）

**請求**：`GET /api/movies/999999/available-dates`

**回應**：
```json
{
  "success": false,
  "message": "找不到 ID 為 999999 的電影",
  "data": null,
  "errors": null
}
```

**驗證結果**：
- ✅ HTTP 404 Not Found
- ✅ `success` 為 `false`
- ✅ 錯誤訊息清楚明確
- ✅ `data` 為 `null`

---

### 測試執行記錄

![API 測試執行截圖](/api_test_result.webp)

---

## 🎯 測試建議

### 手動測試步驟

1. 確保資料庫有測試資料：
   - 至少一部電影
   - 該電影有場次
   - 場次的日期有 `DailySchedule` 記錄且狀態為 `OnSale`

2. 使用 VS Code REST Client 或 Postman 執行 [`get-available-dates.http`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/訂票API-選擇日期/tests/get-available-dates.http) 中的測試

3. 驗證回應：
   - ✅ 返回電影完整資訊
   - ✅ 返回可訂票日期列表
   - ✅ 日期按升序排序
   - ✅ 星期為繁體中文

### 測試場景

| 測試場景 | 預期結果 |
|---------|---------|
| 電影存在且有 OnSale 的場次 | 200 OK，返回日期列表 |
| 電影不存在 | 404 Not Found |
| 電影存在但無 OnSale 的場次 | 200 OK，返回空日期列表 |
| 無效 ID（負數） | 404 Not Found |
