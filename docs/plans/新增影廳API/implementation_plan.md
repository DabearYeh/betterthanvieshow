# 新增影廳 API 實作計畫（含座位配置）

## 目標

開發 `POST /api/admin/theaters` 端點，讓管理者可以建立新的影廳，**同時提供完整的座位配置**（使用二維陣列格式）。

## UI 參考

根據提供的 UI 設計，新增影廳功能包含：

**基本資訊**：
- 影廳名稱（文字輸入）
- 影廳類型（下拉選單：一般數位、4DX、IMAX）
- 座位數（顯示為 rowCount x columnCount）
- 樓層（數字輸入）

**座位配置**（右側面板）：
- 排數（8）：可透過 + - 按鈕調整
- 列數（15）：可透過 + - 按鈕調整
- 座位網格：可選擇一般座位、殘障座位、走道、Empty

> [!IMPORTANT]
> **最終實作**：前端在建立影廳時會**同時傳送座位配置**，使用**二維陣列格式**指定每個位置的座位類型。

---

## 提議的變更

### Entity 層

#### [NEW] [Seat.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Seat.cs)

建立 Seat 實體來儲存每個座位的詳細資訊。

**欄位定義**：
- `Id`: 座位 ID（主鍵）
- `TheaterId`: 所屬影廳 ID（外鍵）
- `RowName`: 座位排名（A, B, C...）
- `ColumnNumber`: 欄號（1, 2, 3...）
- `SeatType`: 座位類型（一般座位、殘障座位、走道、Empty）
- `IsValid`: 是否有效（Empty = false）

---

### 資料庫配置

#### [MODIFY] [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)

新增 Seats DbSet 和實體配置。

**重點配置**：
1. **外鍵關聯**：Seat N:1 Theater，Cascade Delete
2. **唯一約束**：同一影廳內 (RowName, ColumnNumber) 必須唯一

**Migration**: `AddSeatTable`

---

### DTO 層

#### [MODIFY] [CreateTheaterRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CreateTheaterRequestDto.cs)

修改請求 DTO 以接受座位配置二維陣列。

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

**驗證規則**：
- `seats` 陣列行數必須等於 `rowCount`
- 每一行的列數必須等於 `columnCount`
- 座位類型必須是有效值（一般座位、殘障座位、走道、Empty）

---

### Repository 層

#### [MODIFY] [ITheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITheaterRepository.cs) & [TheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TheaterRepository.cs)

新增兩個方法支援座位建立。

**新增方法**：

1. **CreateSeatsAsync**：
   ```csharp
   Task CreateSeatsAsync(int theaterId, List<Seat> seats, int totalSeats);
   ```
   - 批次新增座位
   - 更新 Theater 的 TotalSeats

2. **GetByIdAsync**：
   ```csharp
   Task<Theater> GetByIdAsync(int id);
   ```
   - 根據 ID 取得影廳

---

### Service 層

#### [MODIFY] [TheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)

大幅修改 `CreateTheaterAsync` 以處理座位建立。

**處理流程**：

1. **驗證座位陣列尺寸**：
   - 檢查 `seats.Count == rowCount`
   - 檢查每一行 `row.Count == columnCount`

2. **建立 Theater**：
   - 先將 TotalSeats 設為 0

3. **自動產生座位**：
   ```csharp
   for (int row = 0; row < rowCount; row++)
   {
       string rowName = ((char)('A' + row)).ToString();  // A, B, C...
       for (int col = 0; col < columnCount; col++)
       {
           var seat = new Seat
           {
               TheaterId = createdTheater.Id,
               RowName = rowName,
               ColumnNumber = col + 1,  // 1, 2, 3...
               SeatType = seats[row][col],
               IsValid = seats[row][col] != "Empty"
           };
       }
   }
   ```

4. **計算 TotalSeats**：
   ```csharp
   if (seatType == "一般座位" || seatType == "殘障座位")
   {
       actualSeatCount++;
   }
   ```

5. **批次儲存**：
   - 使用 `CreateSeatsAsync` 批次新增所有座位
   - 同時更新 Theater 的 TotalSeats

---

### Controller 層

#### [MODIFY] [TheatersController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TheatersController.cs)

POST 端點保持不變，但現在會處理包含座位陣列的請求。

**端點規格**：
- **路由**: `POST /api/admin/theaters`
- **授權**: Admin 角色
- **請求**: `CreateTheaterRequestDto`（含 seats 陣列）
- **回應**: 
  - 201 Created: 成功建立
  - 400 Bad Request: 驗證失敗（含詳細錯誤）
  - 500 Internal Server Error: 伺服器錯誤

---

## 驗證計畫

### 手動驗證

#### 測試案例 1：成功建立影廳與座位
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
- `totalSeats: 9`（6個一般 + 2個殘障 + 1個一般）
- 資料庫建立 1 個 Theater 和 15 個 Seat 記錄

#### 測試案例 2：座位陣列尺寸不符
**請求**: rowCount=3 但 seats 只有 2 行

**預期結果**: 400 Bad Request，錯誤訊息指出陣列尺寸不符

#### 測試案例 3：缺少必填欄位
**請求**: 缺少 `name` 或 `seats`

**預期結果**: 400 Bad Request，errors 包含缺少的欄位

---

## 技術重點

### 座位命名規則
- **排名**：自動產生 A, B, C...（使用 ASCII 碼轉換）
- **欄號**：從 1 開始（1, 2, 3...）

### TotalSeats 計算
只計算可售票座位：
- ✅ 一般座位
- ✅ 殘障座位
- ❌ 走道
- ❌ Empty

### 資料完整性
- **外鍵約束**：座位必須屬於一個影廳
- **唯一約束**：同一影廳內 (RowName, ColumnNumber) 不重複
- **Cascade Delete**：刪除影廳時連帶刪除座位

### 效能優化
- 使用 `AddRangeAsync` 批次新增座位
- 單一交易中完成 Theater 和 Seats 的建立
