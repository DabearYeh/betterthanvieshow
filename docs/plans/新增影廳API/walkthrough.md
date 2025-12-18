# 新增影廳 API 修改 - 加入座位配置

## 需求變更說明

### 原始需求
建立影廳時只需要基本資訊（名稱、類型、樓層、排數、列數），TotalSeats 自動計算為 `rowCount × columnCount`。

### 修改後需求
建立影廳時**同時提供座位配置**，使用二維陣列格式指定每個座位的類型（一般座位、殘障座位、走道、Empty），TotalSeats 計算為**實際座位數量**（一般座位 + 殘障座位）。

---

## 已完成的變更

### Entity 層

#### Seat.cs
建立了全新的 `Seat` 實體來儲存座位資訊。

**位置**: [Seat.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Seat.cs)

**欄位定義**：
- `Id` (int): 座位 ID（主鍵）
- `TheaterId` (int): 所屬影廳 ID（外鍵）
- `RowName` (string): 座位排名（A, B, C...）
- `ColumnNumber` (int): 欄號（1, 2, 3...）
- `SeatType` (string): 座位類型（一般座位、殘障座位、走道、Empty）
- `IsValid` (bool): 座位是否有效（Empty = false，其他 = true）

---

### 資料庫配置

#### ApplicationDbContext.cs
新增了 `Seats` DbSet 和 Seat 實體配置。

**位置**: [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)

**配置重點**：
1. **外鍵關聯**：
   ```csharp
   entity.HasOne(e => e.Theater)
       .WithMany()
       .HasForeignKey(e => e.TheaterId)
       .OnDelete(DeleteBehavior.Cascade);  // 刪除影廳時連帶刪除座位
   ```

2. **唯一約束**：
   ```csharp
   entity.HasIndex(e => new { e.TheaterId, e.RowName, e.ColumnNumber })
       .IsUnique()
       .HasDatabaseName("IX_Seat_Theater_Row_Column");
   ```
   確保同一影廳內 (RowName, ColumnNumber) 組合唯一。

**Migration**: `AddSeatTable`

---

### DTO 層

#### CreateTheaterRequestDto.cs
修改了請求 DTO，加入座位配置二維陣列。

**位置**: [CreateTheaterRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CreateTheaterRequestDto.cs)

**新增欄位**：
```csharp
[Required(ErrorMessage = "座位配置為必填")]
public List<List<string>> Seats { get; set; } = new();
```

**請求範例**：
```json
{
  "name": "廳 A",
  "type": "IMAX",
  "floor": 2,
  "rowCount": 3,
  "columnCount": 5,
  "seats": [
    ["一般座位", "一般座位", "走道", "一般座位", "一般座位"],
    ["一般座位", "殘障座位", "走道", "殘障座位", "一般座位"],
    ["Empty", "一般座位", "走道", "一般座位", "Empty"]
  ]
}
```

---

### Repository 層

#### ITheaterRepository.cs & TheaterRepository.cs
新增了兩個方法來支援座位建立功能。

**位置**：
- [ITheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITheaterRepository.cs)
- [TheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TheaterRepository.cs)

**新增方法**：

1. **CreateSeatsAsync**：
   ```csharp
   Task CreateSeatsAsync(int theaterId, List<Seat> seats, int totalSeats);
   ```
   - 批次新增座位到資料庫
   - 更新影廳的 TotalSeats

2. **GetByIdAsync**：
   ```csharp
   Task<Theater> GetByIdAsync(int id);
   ```
   - 根據 ID 取得影廳資料

---

### Service 層

#### TheaterService.cs
大幅修改了 `CreateTheaterAsync` 方法來處理座位建立。

**位置**: [TheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)

**處理流程**：

1. **驗證座位陣列尺寸**：
   ```csharp
   if (request.Seats.Count != request.RowCount)
   {
       return ApiResponse<TheaterResponseDto>.FailureResponse(
           $"座位陣列排數 ({request.Seats.Count}) 與 RowCount ({request.RowCount}) 不符"
       );
   }
   ```

2. **建立影廳實體** (TotalSeats 暫設為 0)：
   ```csharp
   var theater = new Theater
   {
       Name = request.Name,
       Type = request.Type,
       Floor = request.Floor,
       RowCount = request.RowCount,
       ColumnCount = request.ColumnCount,
       TotalSeats = 0
   };
   ```

3. **自動產生座位實體**：
   ```csharp
   for (int row = 0; row < request.RowCount; row++)
   {
       string rowName = ((char)('A' + row)).ToString(); // A, B, C...
       
       for (int col = 0; col < request.ColumnCount; col++)
       {
           string seatType = request.Seats[row][col];
           
           var seat = new Seat
           {
               TheaterId = createdTheater.Id,
               RowName = rowName,
               ColumnNumber = col + 1,  // 1, 2, 3...
               SeatType = seatType,
               IsValid = seatType != "Empty"
           };
           
           seats.Add(seat);
           
           // 計算實際座位數（一般座位 + 殘障座位）
           if (seatType == "一般座位" || seatType == "殘障座位")
           {
               actualSeatCount++;
           }
       }
   }
   ```

4. **批次儲存座位並更新 TotalSeats**：
   ```csharp
   await _theaterRepository.CreateSeatsAsync(createdTheater.Id, seats, actualSeatCount);
   ```

---

## API 規格變更

### 請求格式

**端點**: `POST /api/admin/theaters`

**請求 Body**:
```json
{
  "name": "廳 A",
  "type": "IMAX",
  "floor": 2,
  "rowCount": 3,
  "columnCount": 5,
  "seats": [
    ["一般座位", "一般座位", "走道", "一般座位", "一般座位"],
    ["一般座位", "殘障座位", "走道", "殘障座位", "一般座位"],
    ["Empty", "一般座位", "走道", "一般座位", "Empty"]
  ]
}
```

**重要規則**：
- `seats` 陣列必須是 `rowCount` 行 × `columnCount` 列
- 每個座位類型必須是：`一般座位`、`殘障座位`、`走道`、`Empty` 之一
- `TotalSeats` 會自動計算為「一般座位」和「殘障座位」的總數

---

### 回應格式

**成功回應** (201 Created):
```json
{
  "success": true,
  "message": "影廳建立成功",
  "data": {
    "id": 1,
    "name": "廳 A",
    "type": "IMAX",
    "floor": 2,
    "rowCount": 3,
    "columnCount": 5,
    "totalSeats": 9  // 實際座位數（一般座位 + 殘障座位）
  }
}
```

**驗證失敗回應** (400 Bad Request):
```json
{
  "success": false,
  "message": "座位陣列排數 (2) 與 RowCount (3) 不符",
  "errors": null
}
```

---

## 資料庫 Schema

### Theater 表
| 欄位 | 類型 | 說明 |
|------|------|------|
| Id | int | 主鍵 |
| Name | string | 影廳名稱 |
| Type | string | 影廳類型 |
| Floor | int | 樓層 |
| RowCount | int | 排數 |
| ColumnCount | int | 列數 |
| TotalSeats | int | **實際座位總數**（一般 + 殘障） |

### Seat 表（新增）
| 欄位 | 類型 | 說明 |
|------|------|------|
| Id | int | 主鍵 |
| TheaterId | int | 外鍵 → Theater.Id |
| RowName | string | 排名（A, B, C...） |
| ColumnNumber | int | 欄號（1, 2, 3...） |
| SeatType | string | 座位類型 |
| IsValid | bool | 是否有效 |

**唯一約束**: `(TheaterId, RowName, ColumnNumber)` 組合必須唯一

---

## 驗證結果

### ✅ 程式碼編譯成功

執行 `dotnet build` 成功，無編譯錯誤。

### ✅ Migration 建立成功

成功建立 `AddSeatTable` Migration，包含 Seat 資料表及所有約束。

---

## 測試指引

### 測試案例 1：成功建立影廳與座位

**請求**:
```json
{
  "name": "廳 A",
  "type": "IMAX",
  "floor": 2,
  "rowCount": 3,
  "columnCount": 5,
  "seats": [
    ["一般座位", "一般座位", "走道", "一般座位", "一般座位"],
    ["一般座位", "殘障座位", "走道", "殘障座位", "一般座位"],
    ["Empty", "一般座位", "走道", "一般座位", "Empty"]
  ]
}
```

**預期結果**:
- 201 Created
- `totalSeats: 9` （6個一般座位 + 2個殘障座位 + 1個一般座位 = 9）
- 資料庫建立 1 個 Theater 記錄
- 資料庫建立 15 個 Seat 記錄（3×5）

### 測試案例 2：座位陣列尺寸不符

**請求**:
```json
{
  "name": "廳 B",
  "type": "一般數位",
  "floor": 1,
  "rowCount": 3,
  "columnCount": 5,
  "seats": [
    ["一般座位", "一般座位"]  // 只有 2 列，應該要 5 列
  ]
}
```

**預期結果**:
- 400 Bad Request
- 錯誤訊息指出陣列尺寸不符

---

## 技術重點

### 座位自動命名
- **排名**：自動產生 A, B, C...（使用 ASCII 碼轉換）
- **欄號**：從 1 開始編號

### TotalSeats 計算邏輯
只計算**可售票**的座位：
- ✅ 一般座位
- ✅ 殘障座位
- ❌ 走道
- ❌ Empty

### 資料完整性
- 外鍵約束確保座位必須屬於一個影廳
- 唯一約束確保同一影廳內座位位置不重複
- Cascade Delete：刪除影廳時自動刪除所有座位

### 批次處理
使用 `AddRangeAsync` 批次新增座位，提升效能。
