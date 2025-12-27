# 取得特定日期場次列表 API - 實作完成

## 📋 實作摘要

成功實作第二支訂票 API：`GET /api/movies/{movieId}/showtimes?date={date}`

此 API 用於訂票流程的第二步，讓使用者選擇日期後查看該電影在該日期有哪些場次可以訂票，包含影廳資訊、時間、票價和座位數。

---

## ✅ 完成項目

### 1. 實體層
- ✅ 建立 [`Ticket.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Ticket.cs) 實體
  - 包含票券編號、訂單 ID、場次 ID、座位 ID、QR Code、狀態、票價等屬性
  - 建立與 `MovieShowTime` 和 `Seat` 的導航屬性
- ✅ 更新 [`ApplicationDbContext.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)
  - 新增 `Tickets` DbSet
  - 配置 Ticket 實體的約束和索引

### 2. Repository 層
- ✅ 建立 [`ITicketRepository.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITicketRepository.cs) 介面
- ✅ 建立 [`TicketRepository.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TicketRepository.cs) 實作
  - 實作 `GetSoldTicketCountByShowTimeAsync` 方法查詢已售出票券數
  - 只計算有效票券（待支付、未使用、已使用），不包含已過期
- ✅ 擴展 [`ShowtimeRepository.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs#L130-L150)
  - 新增 `GetShowtimesByMovieAndDateAsync` 方法
  - 使用 `JOIN` 查詢 `DailySchedules` 確保只返回 `OnSale` 狀態的場次
  - 使用 `Include` 載入關聯的 `Movie` 和 `Theater` 資料

### 3. DTO 層
- ✅ 建立 [`MovieShowtimesResponseDto.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/MovieShowtimesResponseDto.cs)
  - `MovieShowtimesResponseDto`：包含電影 ID、名稱、日期和場次列表
  - `ShowtimeListItemDto`：場次項目，包含影廳、時間、票價、座位資訊

### 4. Service 層
- ✅ 擴展 [`IMovieService.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IMovieService.cs#L58-L64) 介面
- ✅ 擴展 [`MovieService.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs#L458-L526)
  - 注入 `ITicketRepository` 依賴
  - 實作 `GetShowtimesByDateAsync` 方法
  - 動態計算結束時間（開始時間 + 電影時長）
  - 動態計算可用座位數（總座位數 - 已售出票券數）
  - 根據影廳類型決定票價（一般數位 300元、4DX/IMAX 380元）

### 5. Controller 層
- ✅ 擴展 [`MoviesController.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs#L203-L289)
  - 新增 `GetShowtimesByDate` 端點
  - 路由：`GET /api/movies/{id}/showtimes?date={date}`
  - 無需授權（`[AllowAnonymous]`）
  - 日期格式驗證（必須為 `YYYY-MM-DD`）
  - 完整的 XML 文件註解和錯誤處理

### 6. 依賴注入
- ✅ 在 [`Program.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs#L64) 註冊 `ITicketRepository`

### 7. 資料庫遷移
- ✅ 建立並執行 EF Core 遷移
  - 遷移名稱：`AddTicketEntity`
  - 新增 `Ticket` 表及相關約束

### 8. HTTP 測試
- ✅ 建立 [`get-showtimes-by-date.http`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/訂票API-選擇場次/tests/get-showtimes-by-date.http) 測試檔案

---

## 🏗️ 技術實作細節

### Repository 層查詢邏輯

```csharp
public async Task<List<MovieShowTime>> GetShowtimesByMovieAndDateAsync(int movieId, DateTime date)
{
    return await _context.MovieShowTimes
        .Include(st => st.Movie)
        .Include(st => st.Theater)
        .Where(st => st.MovieId == movieId && st.ShowDate.Date == date.Date)
        .Join(
            _context.DailySchedules,
            st => st.ShowDate.Date,
            ds => ds.ScheduleDate.Date,
            (st, ds) => new { ShowTime = st, ds.Status }
        )
        .Where(x => x.Status == "OnSale")
        .Select(x => x.ShowTime)
        .OrderBy(st => st.StartTime)
        .ToListAsync();
}
```

**關鍵要點**：
- 使用 `Include` 預先載入關聯資料，避免 N+1 查詢問題
- 使用 `JOIN` 確保只返回 `OnSale` 狀態的場次
- 按開始時間升序排序

### Service 層業務邏輯

1. **驗證電影存在性**：若電影不存在返回 `null`（Controller 回傳 404）
2. **查詢場次**：呼叫 Repository 取得符合條件的場次
3. **動態計算資訊**：
   - 結束時間 = 開始時間 + 電影時長
   - 已售出票券數 = 查詢 Ticket 表中有效票券
   - 可用座位數 = 總座位數 - 已售出票券數
   - 票價 = 根據影廳類型映射
4. **組裝回應 DTO**

### API 回應格式

```json
{
  "success": true,
  "message": "成功取得場次列表",
  "data": {
    "movieId": 2,
    "movieTitle": "復仇者聯盟",
    "date": "2025-12-31",
    "showtimes": [
      {
        "showTimeId": 7,
        "theaterName": "IMAX 3D Theatre",
        "theaterType": "IMAX",
        "startTime": "10:00",
        "endTime": "13:01",
        "price": 380,
        "availableSeats": 10,
        "totalSeats": 10
      }
    ]
  }
}
```

---

## 🧪 測試結果

### 測試執行摘要

已完成 API 的實際測試驗證，所有測試場景通過 ✅

#### 測試 1: 成功取得場次列表

**請求**：`GET /api/movies/2/showtimes?date=2025-12-31`

**回應**：
```json
{
  "success": true,
  "message": "成功取得場次列表",
  "data": {
    "movieId": 2,
    "movieTitle": "復仇者聯盟",
    "date": "2025-12-31",
    "showtimes": [
      {
        "showTimeId": 7,
        "theaterName": "IMAX 3D Theatre",
        "theaterType": "IMAX",
        "startTime": "10:00",
        "endTime": "13:01",
        "price": 380,
        "availableSeats": 10,
        "totalSeats": 10
      }
    ]
  }
}
```

**驗證結果**：
- ✅ HTTP 200 OK
- ✅ 返回電影資訊（ID、名稱）
- ✅ 返回查詢日期
- ✅ 返回場次列表
- ✅ 場次包含所有必要資訊（影廳、時間、票價、座位）
- ✅ 時間格式正確（HH:mm）
- ✅ 票價根據影廳類型正確計算（IMAX = 380元）

---

#### 測試 2: 電影不存在（電影 ID: 999999）

**請求**：`GET /api/movies/999999/showtimes?date=2025-12-31`

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

#### 測試 3: 日期格式無效

**請求**：`GET /api/movies/2/showtimes?date=2025/12/31`

**回應**：
```json
{
  "success": false,
  "message": "日期格式無效，請使用 YYYY-MM-DD 格式",
  "data": null,
  "errors": null
}
```

**驗證結果**：
- ✅ HTTP 400 Bad Request
- ✅ `success` 為 `false`
- ✅ 錯誤訊息提示正確的日期格式
- ✅ `data` 為 `null`

---

## 🔧 問題排查與解決

### 問題 1: 資料庫錯誤 - Invalid object name 'Ticket'

**現象**：
- API 測試時返回 500 錯誤
- 後端日誌顯示：`Invalid object name 'Ticket'`

**原因**：
- 新增了 `Ticket` 實體，但未執行資料庫遷移
- 資料庫中不存在 `Ticket` 表

**解決方案**：
```bash
dotnet ef migrations add AddTicketEntity
dotnet ef database update
```

**結果**：✅ 問題解決，API 正常運作

---

## 📝 業務規則實作

根據 [`瀏覽場次.feature`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/瀏覽場次.feature) 的規則：

> [!NOTE]
> **實作的業務規則**
> 
> - ✅ 只顯示販售中狀態的場次（`DailySchedule.Status = "OnSale"`）
> - ✅ 場次顯示影廳資訊（名稱、類型）
> - ✅ 場次顯示放映時間（開始時間、結束時間）
> - ✅ 場次顯示可用座位數（總座位數 - 已售出票券數）
> - ✅ 票價根據影廳類型決定（一般數位 300元、4DX 380元、IMAX 380元）
> - ✅ 場次按開始時間升序排序

---

## 📌 測試建議

### 測試場景

| 測試場景 | 預期結果 | 實際結果 | 狀態 |
|---------|---------|---------|------|
| 電影存在且有 OnSale 的場次 | 200 OK，返回場次列表 | ✅ 符合 | **PASS** |
| 電影不存在 | 404 Not Found | ✅ 符合 | **PASS** |
| 日期格式錯誤 | 400 Bad Request | ✅ 符合 | **PASS** |
| 電影存在但該日期無場次 | 200 OK，返回空列表 | - | 未測試 |

---

## 🎉 總結

第二支訂票 API 已成功實作並測試完成！

**主要成就**：
- ✅ 建立完整的票券管理基礎（Ticket 實體和 Repository）
- ✅ 實作動態計算邏輯（結束時間、可用座位數、票價）
- ✅ 完善的錯誤處理和驗證
- ✅ 所有測試場景通過

**下一步**：
- 第三支 API：`GET /api/showtimes/{showTimeId}/seats` - 取得場次的座位配置
- 第四支 API：`POST /api/orders` - 建立訂單（訂票）
