# 查詢單一影廳 API 實作計畫

## 目標

開發 `GET /api/admin/theaters/{id}` 端點，回傳影廳名稱與座位表資訊，供管理後台顯示座位配置圖。

## UI 參考

根據提供的 UI 設計圖：

- 標題顯示「{影廳名稱} 座位表」
- 座位表以二維網格呈現
- 列標示: A, B, C, D, E, F, G, H...
- 欄標示: 1, 2, 3, 4, 5...
- 座位類型以不同圖示區分：
  - 🟦 一般座位
  - ♿ 殘障座位  
  - ➖ 走道
  - ⬜ Empty

---

## Proposed Changes

### DTO 層

#### [NEW] TheaterDetailResponseDto.cs

新增影廳詳細資訊回應 DTO：

```csharp
public class TheaterDetailResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }  // 影廳名稱
    public int RowCount { get; set; }  // 排數
    public int ColumnCount { get; set; }  // 列數
    public List<List<SeatDto>> SeatMap { get; set; }  // 座位表（二維陣列）
}

public class SeatDto
{
    public string RowName { get; set; }  // 排名 (A, B, C...)
    public int ColumnNumber { get; set; }  // 欄號 (1, 2, 3...)
    public string SeatType { get; set; }  // 座位類型
}
```

---

### Repository 層

#### [MODIFY] ITheaterRepository.cs

新增方法：
```csharp
Task<Theater?> GetByIdWithSeatsAsync(int id);
```

#### [MODIFY] TheaterRepository.cs

實作 `GetByIdWithSeatsAsync`：
- 使用 `Include(t => t.Seats)` 載入關聯座位
- 回傳 `Theater` 及其所有 `Seat`

---

### Service 層

#### [MODIFY] ITheaterService.cs

新增方法：
```csharp
Task<ApiResponse<TheaterDetailResponseDto>> GetTheaterByIdAsync(int id);
```

#### [MODIFY] TheaterService.cs

實作 `GetTheaterByIdAsync`：將座位轉換為二維陣列格式回傳

---

### Controller 層

#### [MODIFY] TheatersController.cs

新增 `GetTheaterById` 端點：

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetTheaterById(int id)
```

**回應狀態碼**：
- `200 OK`: 查詢成功
- `404 Not Found`: 影廳不存在
- `401 Unauthorized`: 未授權
- `403 Forbidden`: 非 Admin 角色

---

## API 回應範例

**成功回應 (200 OK)**：
```json
{
  "success": true,
  "message": "查詢成功",
  "data": {
    "id": 1,
    "name": "鳳廳",
    "rowCount": 8,
    "columnCount": 13,
    "seatMap": [
      [
        {"rowName": "A", "columnNumber": 1, "seatType": "一般座位"},
        {"rowName": "A", "columnNumber": 2, "seatType": "一般座位"},
        ...
      ]
    ]
  }
}
```
