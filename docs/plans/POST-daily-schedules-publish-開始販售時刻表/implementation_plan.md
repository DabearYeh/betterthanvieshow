# POST /api/admin/daily-schedules/{date}/publish API 實作計畫

實作「開始販售時刻表」API，讓管理者可以將某日期的時刻表從 Draft 狀態轉為 OnSale 狀態。

## User Review Required

> [!WARNING]
> 此 API 會將時刻表狀態從 Draft 改為 OnSale，**此操作不可逆**，販售後無法再編輯該日期的場次。

> [!IMPORTANT]
> 根據規格書，**即使該日期沒有任何場次，也允許開始販售**（空時刻表也可以 OnSale）。

---

## Proposed Changes

### Repository Layer

#### [MODIFY] [IDailyScheduleRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IDailyScheduleRepository.cs)

新增 `UpdateAsync` 方法：
```csharp
/// <summary>
/// 更新每日時刻表
/// </summary>
Task<DailySchedule> UpdateAsync(DailySchedule dailySchedule);
```

#### [MODIFY] [DailyScheduleRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/DailyScheduleRepository.cs)

實作 `UpdateAsync` 方法

---

### Service Layer

#### [MODIFY] [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs)

新增方法：
```csharp
/// <summary>
/// 開始販售時刻表
/// </summary>
/// <param name="date">時刻表日期</param>
Task<DailyScheduleResponseDto> PublishDailyScheduleAsync(DateTime date);
```

#### [MODIFY] [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)

**業務邏輯**：

1. **檢查 DailySchedule 是否存在**
   - 不存在 → 404 Not Found
2. **檢查狀態**
   - 已是 OnSale → 直接返回成功（冪等性）或 409 Conflict（依需求決定）
   - 是 Draft → 繼續
3. **更新狀態**
   - Status 改為 "OnSale"
   - UpdatedAt 更新為當前時間
4. **載入場次資料並回傳**

---

### Controller Layer

#### [MODIFY] [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

新增端點：

```http
POST /api/admin/daily-schedules/{date}/publish
Authorization: Bearer {admin_token}
```

**完整 XML 註解範例**：
```csharp
/// <summary>
/// 開始販售時刻表
/// </summary>
/// <remarks>
/// 將指定日期的時刻表狀態從 Draft 轉為 OnSale。
/// 
/// **重要事項**：
/// - 販售後該日期的場次無法再編輯、新增或刪除
/// - 此操作不可逆，OnSale 無法轉回 Draft
/// - 即使該日期沒有任何場次，也允許開始販售
/// - 已是 OnSale 狀態時，返回成功（冪等性）
/// 
/// **前端應實作二次確認**：
/// 建議前端顯示確認對話框，提醒使用者販售後無法再編輯。
/// </remarks>
/// <param name="date">時刻表日期，格式：YYYY-MM-DD</param>
/// <response code="200">開始販售成功</response>
/// <response code="404">該日期沒有時刻表記錄</response>
/// <response code="401">未授權</response>
```

**Response Body**：
```json
{
  "scheduleDate": "2025-12-23T00:00:00",
  "status": "OnSale",
  "showtimes": [...],
  "createdAt": "2025-12-22T10:00:00",
  "updatedAt": "2025-12-22T17:58:00"
}
```

---

## Verification Plan

### HTTP Tests (betterthanvieshow.http)

| 測試案例 | 預期結果 |
|----------|----------|
| 開始販售 Draft 狀態時刻表 | 200 OK，status 改為 OnSale |
| 開始販售已是 OnSale 的時刻表 | 200 OK（冪等） |
| 開始販售不存在的日期 | 404 Not Found |
| 未授權 | 401 Unauthorized |
| 販售後嘗試儲存場次 | 403 Forbidden（已在現有 API 實作） |

### 測試步驟

```powershell
# 1. 啟動應用程式
dotnet run --project betterthanvieshow

# 2. 使用 betterthanvieshow.http 測試
```

---

## 注意事項

1. **冪等性設計**：重複呼叫 publish 應返回成功，不應報錯
2. **空時刻表**：即使沒有場次，也允許販售（規格書明確要求）
3. **不可逆**：沒有「取消販售」或「回到草稿」的 API
4. **前端確認**：後端不負責二次確認，由前端 UI 處理
