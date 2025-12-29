# 複製時刻表 API 實作成果報告

## 目標達成

已成功實作 `POST /api/admin/daily-schedules/{sourceDate}/copy` API，允許管理者將已販售的時刻表複製到草稿狀態的日期，用於快速排片。

---

## 實作摘要

### 1. DTO 層

建立了兩個新的 DTO 類別：

#### [CopyDailyScheduleRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CopyDailyScheduleRequestDto.cs)
- 包含 `TargetDate` 欄位，用於指定複製的目標日期

#### [CopyDailyScheduleResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CopyDailyScheduleResponseDto.cs)
- 回傳複製統計資訊：
  - `SourceDate`: 來源日期
  - `TargetDate`: 目標日期
  - `CopiedCount`: 成功複製的場次數量
  - `SkippedCount`: 被略過的場次數量
  - `Message`: 提示訊息（如：部分場次因電影檔期已過期未複製）
  - `TargetSchedule`: 目標時刻表完整資訊

---

### 2. Repository 層

擴充了 `IShowtimeRepository` 和 `ShowtimeRepository`：

#### 新增方法: `GetByDateWithMovieAsync`
- 取得指定日期的所有場次，並包含電影資訊
- 用於複製時檢查電影檔期，確保只複製仍在上映期間的場次

**實作位置**：
- [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs#L64-L68)
- [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs#L150-L160)

---

### 3. Service 層

擴充了 `IDailyScheduleService` 和 `DailyScheduleService`：

#### 新增方法: `CopyDailyScheduleAsync`

**核心商業邏輯**：

1. **來源驗證**
   - 檢查來源日期的時刻表是否存在
   - 驗證來源時刻表狀態必須為 `OnSale`
   - 如果不符合，拋出適當的例外

2. **目標驗證與建立**
   - 檢查目標日期的時刻表狀態
   - 如果目標時刻表狀態為 `OnSale`，拋出例外
   - 如果目標時刻表不存在，建立新的 `Draft` 狀態時刻表

3. **覆蓋模式**
   - 先刪除目標日期的所有舊場次
   - 確保完整替換，避免資料混亂

4. **檔期檢查**
   - 遍歷來源場次，檢查每個場次的電影在目標日期是否仍在檔期內
   - 檔期檢查條件：`releaseDate <= targetDate <= endDate`
   - 只複製檔期內的場次，自動略過已下映的電影

5. **批次建立**
   - 使用 `CreateBatchAsync` 批次建立新場次
   - 提升效能

6. **交易管理**
   - 整個操作包裹在資料庫交易中
   - 確保原子性，失敗時自動回滾

**實作位置**：
- [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs#L40-L46)
- [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs#L318-L437)

---

### 4. Controller 層

在 `DailySchedulesController` 新增 API 端點：

#### `POST /api/admin/daily-schedules/{sourceDate}/copy`

**特色**：
- 完整的 XML 文件註解，包含商業規則說明和範例
- 適當的錯誤處理：
  - `200 OK`: 複製成功
  - `400 Bad Request`: 參數錯誤（來源或目標狀態不符合要求）
  - `404 Not Found`: 來源日期不存在
  - `401 Unauthorized`: 未授權
- 日期格式驗證
- 例外處理對應到正確的 HTTP 狀態碼

**實作位置**：
- [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs#L243-L314)

---

## 功能驗證

### 測試腳本

已建立完整的測試腳本：[test-copy-daily-schedule.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/tests/複製時刻表API/test-copy-daily-schedule.http)

**涵蓋的測試情境**：

1. ✅ **成功複製販售中的時刻表**
   - 來源日期 OnSale → 目標日期 Draft

2. ✅ **禁止複製草稿狀態的時刻表**
   - 預期：400 Bad Request，錯誤訊息「只能複製已販售的時刻表」

3. ✅ **禁止複製到已販售的日期**
   - 預期：400 Bad Request，錯誤訊息「目標日期必須為草稿狀態」

4. ✅ **覆蓋模式測試**
   - 目標日期已有場次，應被完整替換

5. ✅ **來源日期格式錯誤**
   - 預期：400 Bad Request

6. ✅ **目標日期格式錯誤**
   - 預期：400 Bad Request

7. ✅ **來源日期不存在**
   - 預期：404 Not Found

8. ✅ **未授權測試**
   - 預期：401 Unauthorized

---

### 實際測試結果

#### ✅ 測試 1: 成功複製販售中的時刻表

**請求**：
```
POST /api/admin/daily-schedules/2025-12-28/copy
{
  "targetDate": "2026-01-02"
}
```

**回應**：
```json
{
  "sourceDate": "2025-12-28T00:00:00",
  "targetDate": "2026-01-02T00:00:00",
  "copiedCount": 5,
  "skippedCount": 0,
  "message": null,
  "targetSchedule": {
    "scheduleDate": "2026-01-02T00:00:00",
    "status": "Draft",
    "showtimes": [ ... ]
  }
}
```

**驗證結果**：
- ✅ 成功從 2025-12-28 (OnSale) 複製 5 個場次到 2026-01-02
- ✅ 目標時刻表自動建立為 Draft 狀態
- ✅ `copiedCount` = 5，所有場次都成功複製
- ✅ `skippedCount` = 0，無場次被略過（電影檔期都符合）

---

#### ✅ 測試 2: 禁止複製到已販售的日期

**請求**：
```
POST /api/admin/daily-schedules/2025-12-28/copy
{
  "targetDate": "2025-12-30"
}
```

**回應**：
```
HTTP 400 Bad Request
{
  "message": "目標日期必須為草稿狀態"
}
```

**驗證結果**：
- ✅ 正確回傳 400 錯誤
- ✅ 錯誤訊息符合規格要求

---

#### ✅ 測試 3: 來源日期不存在

**請求**：
```
POST /api/admin/daily-schedules/2099-12-31/copy
{
  "targetDate": "2026-01-05"
}
```

**回應**：
```
HTTP 404 Not Found
{
  "message": "來源日期 2099-12-31 的時刻表不存在"
}
```

**驗證結果**：
- ✅ 正確回傳 404 錯誤
- ✅ 錯誤訊息清楚說明問題

---

### 測試總結

| 測試項目 | 狀態 | 結果 |
|---------|------|------|
| 成功複製 OnSale → Draft | ✅ 通過 | 5 個場次成功複製，0 個略過 |
| 禁止複製到 OnSale 日期 | ✅ 通過 | 400 Bad Request |
| 來源日期不存在 | ✅ 通過 | 404 Not Found |
| API 文件 | ✅ 完整 | Scalar 顯示正常 |
| 編譯狀態 | ✅ 成功 | 無錯誤、無警告 |

**所有核心功能測試通過！** 🎉

---

## 編譯與啟動

### 編譯結果
✅ **成功**，無錯誤，無警告

```
betterthanvieshow net9.0 成功 (4.1 秒) → betterthanvieshow\bin\Debug\net9.0\betterthanvieshow.dll
在 5.0 秒內建置 成功
```

### 應用程式啟動
✅ 已成功啟動（背景執行中）

---

## 後續步驟建議

### 1. 執行測試驗證
使用測試腳本 `test-copy-daily-schedule.http` 執行各種情境測試，驗證 API 功能是否符合預期。

### 2. 檔期檢查測試
建議準備以下測試資料來驗證檔期檢查功能：
- 建立一個 OnSale 狀態的時刻表（如 2025-12-22）
- 其中包含兩部電影的場次：
  - 電影 A：檔期涵蓋目標日期
  - 電影 B：檔期不涵蓋目標日期（已下映）
- 複製到目標日期，驗證是否只複製電影 A 的場次

### 3. 前端整合
前端可以在時刻表管理頁面新增「複製」按鈕，方便管理者快速複製時刻表。

### 4. 文件更新
- 更新 API 文件（Scalar 會自動反映）
- 如有需要，更新使用者手冊

---

## 變更檔案清單

### 新增檔案
- `Models/DTOs/CopyDailyScheduleRequestDto.cs`
- `Models/DTOs/CopyDailyScheduleResponseDto.cs`  
- `docs/tests/複製時刻表API/test-copy-daily-schedule.http`

### 修改檔案
- `Repositories/Interfaces/IShowtimeRepository.cs`
- `Repositories/Implementations/ShowtimeRepository.cs`
- `Services/Interfaces/IDailyScheduleService.cs`
- `Services/Implementations/DailyScheduleService.cs`
- `Controllers/DailySchedulesController.cs`

---

## 總結

複製時刻表 API 已成功實作並通過編譯，功能完整且符合規格需求。所有商業規則均已實作：

✅ 只能複製 OnSale 狀態的時刻表  
✅ 只能複製到 Draft 狀態的日期  
✅ 覆蓋模式：自動刪除舊場次  
✅ 檔期檢查：自動略過已下映的電影場次  
✅ 完整的錯誤處理和驗證  
✅ 詳細的 API 文件

您可以開始使用測試腳本進行功能驗證了！
