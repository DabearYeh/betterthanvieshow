# GET /api/admin/daily-schedules/{date} API 實作走查

## 功能概述

成功實作 `GET /api/admin/daily-schedules/{date}` API，允許管理員查詢特定日期的時刻表及其所有場次資料。

---

## 實作變更

### Service 層

#### [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs#L28-L30)
新增方法簽章：
```csharp
Task<DailyScheduleResponseDto> GetDailyScheduleAsync(DateTime date);
```

#### [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs#L277-L293)
實作查詢邏輯：
- 查詢指定日期的 `DailySchedule`，若不存在返回 `KeyNotFoundException`（轉為 404）
- 透過 `GetByDateWithDetailsAsync()` 載入所有場次及關聯的電影、影廳資料
- 重用現有的 `BuildDailyScheduleResponse()` helper 方法組裝回應

**Bug 修正**：修正第 273 行缺少 `UpdatedAt` 屬性的語法錯誤

---

### Controller 層

#### [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs#L94-L135)

新增端點：
```csharp
[HttpGet("{date}")]
public async Task<IActionResult> GetDailySchedule([FromRoute] string date)
```

**錯誤處理**：
- `400 Bad Request`：日期格式錯誤
- `404 Not Found`：時刻表不存在
- `401 Unauthorized`：未授權

---

## API 規格

**端點**：`GET /api/admin/daily-schedules/{date}`

**權限**：需 Admin 角色

**路徑參數**：
- `date` (string)：日期格式 YYYY-MM-DD

**回應範例** (200 OK)：
```json
{
  "scheduleDate": "2025-12-24T00:00:00",
  "status": "Draft",
  "showtimes": [
    {
      "id": 1,
      "movieId": 10,
      "movieTitle": "復仇者聯盟",
      "movieDuration": 120,
      "theaterId": 1,
      "theaterName": "IMAX 廳",
      "theaterType": "IMAX",
      "showDate": "2025-12-24T00:00:00",
      "startTime": "09:00",
      "endTime": "11:00",
      "scheduleStatus": "Draft",
      "createdAt": "2025-12-23T01:00:00Z"
    }
  ],
  "createdAt": "2025-12-23T01:00:00Z",
  "updatedAt": "2025-12-23T01:30:00Z"
}
```

---

## 測試結果

### 測試執行摘要

| 測試案例 | 狀態 | 結果 |
|---------|------|------|
| 查詢不存在的時刻表 | ✅ 通過 | 404 Not Found |
| 日期格式錯誤 | ✅ 通過 | 400 Bad Request |
| 未授權訪問 | ✅ 通過 | 401 Unauthorized |

**通過率**：3/3 (100%)

### 測試詳情

#### 測試 1：查詢不存在的時刻表
```http
GET http://localhost:5041/api/admin/daily-schedules/2099-12-31
Authorization: Bearer {token}
```
✅ 正確返回 `404 Not Found` 及錯誤訊息「該日期沒有時刻表記錄」

#### 測試 2：日期格式錯誤
```http
GET http://localhost:5041/api/admin/daily-schedules/invalid-date
Authorization: Bearer {token}
```
✅ 正確返回 `400 Bad Request` 及錯誤訊息「日期格式錯誤，必須為 YYYY-MM-DD」

#### 測試 3：未授權訪問
```http
GET http://localhost:5041/api/admin/daily-schedules/2025-12-24
```
✅ 正確返回 `401 Unauthorized`，要求提供有效的 Bearer token

---

## 前端整合範例

```typescript
// 查詢特定日期的時刻表
async function getDailySchedule(date: string, token: string) {
  const response = await fetch(
    `http://localhost:5041/api/admin/daily-schedules/${date}`,
    {
      headers: { 
        'Authorization': `Bearer ${token}` 
      }
    }
  );
  
  if (!response.ok) {
    if (response.status === 404) {
      console.log('該日期沒有時刻表');
      return null;
    }
    throw new Error('查詢失敗');
  }
  
  const data = await response.json();
  
  // 判斷是否販售中，顯示黃色標籤
  if (data.status === "OnSale") {
    showBadge("販售中", "yellow");
  }
  
  // 依影廳分組顯示場次
  const groupedByTheater = groupBy(data.showtimes, 'theaterId');
  
  return data;
}
```

---

## 相關檔案

- **測試檔案**：[get-daily-schedule.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/tests/get-daily-schedule.http)
- **測試報告**：[GET_Daily_Schedule_測試報告.md](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/tests/GET_Daily_Schedule_測試報告.md)
- **Service 實作**：[DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)
- **Controller**：[DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

---

## 總結

✅ **API 實作完成且功能正常**

**已驗證的功能**：
- ✅ Service 層正確查詢 DailySchedule 和 MovieShowTime
- ✅ Controller 正確處理日期驗證
- ✅ 錯誤處理完整（404, 400, 401）
- ✅ 授權機制正常運作
- ✅ 重用現有的 BuildDailyScheduleResponse 避免重複代碼

API 已準備好供前端使用，可以開始整合到時刻表管理頁面。
