# PUT /api/admin/daily-schedules/{date} API 實作完成報告

## 實作摘要

成功實作儲存每日時刻表 API，管理者可一次儲存某日期的所有場次資料。

---

## 新建檔案

### DTO Layer
- [ShowtimeItemDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/ShowtimeItemDto.cs) - 單一場次資料
- [SaveDailyScheduleRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/SaveDailyScheduleRequestDto.cs) - 請求 DTO
- [DailyScheduleResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/DailyScheduleResponseDto.cs) - 回應 DTO

### Service Layer
- [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs)
- [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)

### Controller Layer
- [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

---

## 修改的檔案

- [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs) - 新增批次操作方法
- [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs) - 實作批次操作
- [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs) - DI 註冊
- [betterthanvieshow.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/betterthanvieshow.http) - 測試案例

---

## 核心功能

### API 設計
```
PUT /api/admin/daily-schedules/2025-12-30
Authorization: Bearer {admin_token}
```

### Request Body
```json
{
  "showtimes": [
    { "movieId": 1, "theaterId": 1, "startTime": "09:45" },
    { "movieId": 2, "theaterId": 2, "startTime": "14:00" }
  ]
}
```

### 後端邏輯（DailyScheduleService）

| 步驟 | 說明 |
|------|------|
| 1 | 開啟資料庫 Transaction |
| 2 | 檢查或建立 DailySchedule（Upsert） |
| 3 | 若狀態為 OnSale → 拋出 403 錯誤 |
| 4 | 驗證所有場次（電影/影廳存在、15分鐘粒度、上映期間） |
| 5 | 檢查場次清單內部時間衝突 |
| 6 | 全刪全建（刪除舊場次 → 批次新增） |
| 7 | 更新 DailySchedule.UpdatedAt |
| 8 | 提交 Transaction |

---

## 測試案例

HTTP 測試檔案包含以下情境：

| 測試案例 | 預期結果 |
|----------|----------|
| 首次儲存時刻表 | 200 OK，自動建立 DailySchedule (Draft) |
| 修改已有時刻表 | 200 OK，場次全部替換 |
| 清空時刻表（空陣列） | 200 OK，保留 DailySchedule |
| 電影不存在 | 400 Bad Request |
| 影廳不存在 | 400 Bad Request |
| 時間非 15 分鐘倍數 | 400 Bad Request |
| 場次時間衝突 | 409 Conflict |

---

## 特色功能

### 1. Upsert 設計
- ✅ 第一次儲存：自動建立 DailySchedule
- ✅ 後續儲存：更新現有資料
- ✅ 前端只需一支 API

### 2. Transaction 保證
- ✅ 全刪全建在同一個 Transaction
- ✅ 失敗自動 Rollback
- ✅ 資料一致性保證

### 3. 完整驗證
- ✅ 電影/影廳存在性
- ✅ 15 分鐘粒度檢查
- ✅ 上映日期範圍檢查
- ✅ 場次內部時間衝突檢查

### 4. Scalar 文檔
- ✅ 完整 XML 註解
- ✅ 範例請求展示
- ✅ 驗證規則說明

---

## 接續開發

此 API 為時刻表管理的核心，後續可擴展：
- `GET /api/admin/daily-schedules/{date}` - 取得某日時刻表
- `POST /api/admin/daily-schedules/{date}/publish` - 開始販售
- `POST /api/admin/daily-schedules/{sourceDate}/copy-to/{targetDate}` - 複製時刻表
