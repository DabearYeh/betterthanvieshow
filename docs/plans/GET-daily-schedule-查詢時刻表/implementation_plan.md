# 新增每日時刻表查詢 API

## 目標

實作 `GET /api/admin/daily-schedules/{date}` API，允許管理員查詢特定日期的時刻表及其所有場次資料。

## User Review Required

> [!IMPORTANT]
> **權限設計確認**
> 
> 目前規劃此 API 僅供 Admin 使用（`/api/admin/` 路徑），因為：
> - Admin 需要查看 Draft 狀態的時刻表來進行編輯
> - 一般使用者（訂票端）應使用另一個公開 API（如 `/api/showtimes?date=xxx`）僅查看 OnSale 狀態的場次
> 
> **請確認：**
> 1. 是否需要同時實作一般使用者用的公開查詢 API？
> 2. 或者此 API 改為公開，但依據使用者角色過濾顯示內容？

---

## Proposed Changes

### API Layer

#### [MODIFY] [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

**新增方法**：
```csharp
/// <summary>
/// 查詢每日時刻表
/// </summary>
/// <param name="date">時刻表日期，格式：YYYY-MM-DD</param>
/// <response code="200">查詢成功</response>
/// <response code="400">日期格式錯誤</response>
/// <response code="404">該日期沒有時刻表記錄</response>
[HttpGet("{date}")]
[ProducesResponseType(typeof(DailyScheduleResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetDailySchedule([FromRoute] string date)
```

**實作邏輯**：
1. 解析並驗證日期格式
2. 呼叫 Service 層查詢
3. 若找不到記錄返回 404
4. 成功返回 `DailyScheduleResponseDto`（包含狀態及場次列表）

---

### Service Layer

#### [MODIFY] IDailyScheduleService.cs

**新增方法簽章**：
```csharp
Task<DailyScheduleResponseDto> GetDailyScheduleAsync(DateTime scheduleDate);
```

#### [MODIFY] [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)

**實作查詢邏輯**：
1. 透過 `_context.DailySchedules` 查詢指定日期的記錄
2. 透過 `_context.MovieShowTimes` 查詢 `show_date = scheduleDate` 的所有場次
3. 對每個場次載入關聯的 `Movie` 和 `Theater` 資料（使用 `.Include()`）
4. 計算每個場次的剩餘座位數：
   - 總座位數 = `Theater.TotalSeats`
   - 已售座位數 = 該場次關聯的 `Ticket` 且 `status IN ('待支付', '未使用', '已使用')`（排除已過期）
   - 剩餘座位數 = 總座位數 - 已售座位數
5. 將場次資料轉換為 `ShowtimeResponseDto`
6. 組裝並返回 `DailyScheduleResponseDto`

---

### DTO Layer

#### [MODIFY] [ShowtimeResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/ShowtimeResponseDto.cs)

**確認現有欄位**（若缺少則新增）：
- `Id`（場次 ID）
- `MovieId`, `MovieTitle`, `MoviePosterUrl`（電影資訊）
- `TheaterId`, `TheaterName`, `TheaterType`（影廳資訊）
- `ShowDate`（放映日期）
- `StartTime`, `EndTime`（開始/結束時間，EndTime 需動態計算）
- `AvailableSeats`（剩餘座位數）

---

## Verification Plan

### Automated Tests

> [!WARNING]
> **目前缺少自動化測試架構**
> 
> 專案中尚未發現單元測試或整合測試檔案。建議在實作後透過手動測試驗證。

### Manual Verification

使用 HTTP 測試檔案驗證以下情境：

#### **測試 1：查詢存在的時刻表（Draft 狀態）**

```http
### 查詢草稿狀態時刻表
GET {{baseUrl}}/api/admin/daily-schedules/2025-12-24
Authorization: Bearer {{adminToken}}
```

**預期結果**：
- HTTP 200
- 回傳 JSON 包含：
  - `scheduleDate`: `"2025-12-24T00:00:00"`
  - `status`: `"Draft"`
  - `showtimes`: 陣列（包含該日期的所有場次及電影/影廳資訊、剩餘座位數）

---

#### **測試 2：查詢存在的時刻表（OnSale 狀態）**

```http
### 查詢販售中狀態時刻表
GET {{baseUrl}}/api/admin/daily-schedules/2025-12-25
Authorization: Bearer {{adminToken}}
```

**預期結果**：
- HTTP 200
- `status`: `"OnSale"`

---

#### **測試 3：查詢不存在的時刻表**

```http
### 查詢不存在的日期
GET {{baseUrl}}/api/admin/daily-schedules/2099-12-31
Authorization: Bearer {{adminToken}}
```

**預期結果**：
- HTTP 404
- 錯誤訊息：`"該日期沒有時刻表記錄"`

---

#### **測試 4：日期格式驗證**

```http
### 日期格式錯誤
GET {{baseUrl}}/api/admin/daily-schedules/invalid-date
Authorization: Bearer {{adminToken}}
```

**預期結果**：
- HTTP 400
- 錯誤訊息：`"日期格式錯誤，必須為 YYYY-MM-DD"`

---

#### **測試 5：權限驗證**

```http
### 未授權訪問
GET {{baseUrl}}/api/admin/daily-schedules/2025-12-24
```

**預期結果**：
- HTTP 401

---

#### **測試 6：驗證場次資料完整性**

在已建立場次的日期下執行測試 1，手動檢查：
- 每個場次的 `endTime` 是否正確計算（`startTime + movie.duration`）
- `availableSeats` 是否正確（若該場次有訂票，數字應為剩餘座位）
- 電影和影廳資訊是否完整載入

---

## 驗證檢查清單

- [ ] API 能正確查詢 Draft 狀態的時刻表
- [ ] API 能正確查詢 OnSale 狀態的時刻表
- [ ] 不存在的日期返回 404
- [ ] 日期格式錯誤返回 400
- [ ] 未授權訪問返回 401
- [ ] 場次資料包含完整的電影、影廳資訊
- [ ] 剩餘座位數計算正確
- [ ] 場次結束時間動態計算正確
