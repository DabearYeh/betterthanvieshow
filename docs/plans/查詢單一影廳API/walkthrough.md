# 查詢單一影廳 API 實作紀錄

## 完成日期
2025-12-20

## 實作摘要

成功開發 `GET /api/admin/theaters/{id}` 端點，回傳影廳名稱與座位表資訊。

---

## 變更檔案

| 檔案 | 變更類型 | 說明 |
|-----|---------|------|
| `Models/DTOs/TheaterDetailResponseDto.cs` | 新增 | 影廳詳細資訊 DTO（含座位表） |
| `Models/Entities/Theater.cs` | 修改 | 新增 `Seats` 導航屬性 |
| `Data/ApplicationDbContext.cs` | 修改 | 修正 Seat 關聯配置 `WithMany(t => t.Seats)` |
| `Repositories/Interfaces/ITheaterRepository.cs` | 修改 | 新增 `GetByIdWithSeatsAsync` 方法 |
| `Repositories/Implementations/TheaterRepository.cs` | 修改 | 實作 `GetByIdWithSeatsAsync` |
| `Services/Interfaces/ITheaterService.cs` | 修改 | 新增 `GetTheaterByIdAsync` 方法 |
| `Services/Implementations/TheaterService.cs` | 修改 | 實作座位表二維陣列轉換邏輯 |
| `Controllers/TheatersController.cs` | 修改 | 新增 `GetTheaterById` 端點 |

---

## 測試結果

| 測試案例 | 預期結果 | 實際結果 | 狀態 |
|---------|---------|---------|------|
| 成功查詢影廳（含座位表） | 200 OK | 200 OK | ✅ PASS |
| 查詢不存在的影廳 | 404 Not Found | 404 Not Found | ✅ PASS |
| 未授權存取 | 401 Unauthorized | 401 Unauthorized | ✅ PASS |

---

## 回應範例

```json
{
  "success": true,
  "message": "查詢成功",
  "data": {
    "id": 2,
    "name": "IMAX 3D Theatre",
    "rowCount": 3,
    "columnCount": 5,
    "seatMap": [
      [
        {"rowName": "A", "columnNumber": 1, "seatType": "一般座位"},
        {"rowName": "A", "columnNumber": 2, "seatType": "一般座位"},
        {"rowName": "A", "columnNumber": 3, "seatType": "走道"},
        {"rowName": "A", "columnNumber": 4, "seatType": "一般座位"},
        {"rowName": "A", "columnNumber": 5, "seatType": "一般座位"}
      ],
      [
        {"rowName": "B", "columnNumber": 1, "seatType": "一般座位"},
        {"rowName": "B", "columnNumber": 2, "seatType": "殘障座位"},
        {"rowName": "B", "columnNumber": 3, "seatType": "走道"},
        {"rowName": "B", "columnNumber": 4, "seatType": "殘障座位"},
        {"rowName": "B", "columnNumber": 5, "seatType": "一般座位"}
      ],
      [
        {"rowName": "C", "columnNumber": 1, "seatType": "Empty"},
        {"rowName": "C", "columnNumber": 2, "seatType": "一般座位"},
        {"rowName": "C", "columnNumber": 3, "seatType": "走道"},
        {"rowName": "C", "columnNumber": 4, "seatType": "一般座位"},
        {"rowName": "C", "columnNumber": 5, "seatType": "Empty"}
      ]
    ]
  }
}
```

---

## 開發過程中的問題與解決

### 問題 1：EF Core Include 排序語法錯誤

**錯誤訊息**：SQL Server Error 207 - Invalid column name

**原因**：在 `Include()` 中使用 `OrderBy()` 語法不被支援

**解決**：移除 Include 中的排序，改為在 Service 層處理

```csharp
// 修正前（錯誤）
.Include(t => t.Seats.OrderBy(s => s.RowName))

// 修正後（正確）
.Include(t => t.Seats)
```

### 問題 2：EF Core 導航屬性配置錯誤

**錯誤訊息**：Invalid column name 'TheaterId1'

**原因**：`ApplicationDbContext` 中 Seat 實體的 `WithMany()` 為空，與新加入的 `Theater.Seats` 導航屬性衝突

**解決**：更新 DbContext 配置

```csharp
// 修正前
.WithMany()

// 修正後
.WithMany(t => t.Seats)
```
