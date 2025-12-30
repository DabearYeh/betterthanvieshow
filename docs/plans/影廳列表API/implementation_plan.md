# 影廳列表 API 實作計畫

## 目標

修改 `GET /api/admin/theaters` API，將座位資訊從「座位總數」改為「一般座位數」和「殘障座位數」分開顯示，以符合前端顯示需求。

## 需求變更說明

根據前端 UI 設計（如下圖所示），影廳資訊需要分別顯示：
- **一般座位數**（Regular Seats）
- **殘障座位數**（Accessible Seats）

![前端座位顯示範例](C:/Users/VivoBook/.gemini/antigravity/brain/3e5b67f0-2ae9-498a-9096-500e903c52cb/uploaded_image_1767103332523.png)

因此需要從原本的 `TotalSeats: number` 改為：
- `RegularSeats: number` - 統計 `SeatType = "Regular"` 且 `IsValid = true` 的座位
- `AccessibleSeats: number` - 統計 `SeatType = "Accessible"` 且 `IsValid = true` 的座位

## 需求審查事項

> [!IMPORTANT]
> **授權需求確認**
> 此 API 是否需要限制僅 Admin 角色可存取？根據規格書，影廳管理屬於後台功能，建議加入 `[Authorize(Roles = "Admin")]` 驗證。

## 提議的變更

本次開發將按照專案現有的分層架構（Entity → Repository → Service → Controller）進行實作。

---

### 資料層 (Data & Repository)

#### [NEW] [Theater.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Theater.cs)

建立 `Theater` 實體模型，對應資料庫 `Theater` 資料表。

**欄位說明**：
- `Id` (int, PK): 影廳 ID
- `Name` (string, required): 影廳名稱
- `Type` (string, required): 影廳類型（一般數位、4DX、IMAX）
- `Floor` (int, required): 所在樓層
- `RowCount` (int, required): 排數（必須 > 0）
- `ColumnCount` (int, required): 列數（必須 > 0）
- `TotalSeats` (int, required): 座位總數（必須 > 0）

**驗證規則**：
- `RowCount` 和 `ColumnCount` 必須大於 0
- `TotalSeats` 必須大於 0

#### [NEW] [ITheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITheaterRepository.cs)

定義影廳資料存取介面，提供 `GetAllAsync()` 方法。

#### [MODIFY] [TheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TheaterRepository.cs)

更新 `GetAllAsync` 方法，使用 `.Include(t => t.Seats)` 載入座位資料，以便計算一般座位和殘障座位數量。

---

### 服務層 (Service & DTO)

#### [MODIFY] [TheaterResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/TheaterResponseDto.cs)

更新影廳回應 DTO，包含以下欄位：
- `Id`: 影廳 ID
- `Name`: 影廳名稱
- `Type`: 影廳類型
- `Floor`: 樓層
- `RowCount`: 排數
- `ColumnCount`: 列數
- `RegularSeats`: 一般座位數（從 Seat 資料表統計 SeatType = "Regular" 且 IsValid = true 的數量）
- `AccessibleSeats`: 殘障座位數（從 Seat 資料表統計 SeatType = "Accessible" 且 IsValid = true 的數量）

#### [NEW] [ITheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITheaterService.cs)

定義影廳服務介面，提供 `GetAllTheatersAsync()` 方法，回傳 `ApiResponse<List<TheaterResponseDto>>`。

#### [MODIFY] [TheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)

更新 `GetAllTheatersAsync` 方法，負責：
1. 呼叫 Repository 取得所有影廳（使用 `.Include(t => t.Seats)` 載入座位資料）
2. 對每個影廳統計座位數量：
   - `RegularSeats`: `Seats.Count(s => s.SeatType == "Regular" && s.IsValid)`
   - `AccessibleSeats`: `Seats.Count(s => s.SeatType == "Accessible" && s.IsValid)`
3. 將 Entity 轉換為 DTO
4. 包裝成 `ApiResponse` 格式回傳

---

### 控制器層 (Controller)

#### [NEW] [TheaterController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TheaterController.cs)

建立影廳控制器，實作 `GET /api/admin/theaters` 端點。

**端點規格**：
- **HTTP Method**: GET
- **路由**: `/api/admin/theaters`
- **授權**: `[Authorize(Roles = "Admin")]`
- **回應格式**:
  ```json
  {
    "success": true,
    "message": "查詢成功",
    "data": [
      {
        "id": 1,
        "name": "廳 A",
        "type": "IMAX",
        "floor": 2,
        "rowCount": 10,
        "columnCount": 12,
        "regularSeats": 48,
        "accessibleSeats": 4
      }
    ]
  }
  ```

---

### 依賴注入配置

#### [MODIFY] [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs#L45-L48)

在服務註冊區塊加入：
```csharp
builder.Services.AddScoped<ITheaterRepository, TheaterRepository>();
builder.Services.AddScoped<ITheaterService, TheaterService>();
```

---

### 資料庫 Migration

#### [NEW] Migration 檔案

執行以下指令建立 Migration：
```bash
dotnet ef migrations add CreateTheaterTable
```

此 Migration 將建立 `Theater` 資料表，包含所有必要欄位及驗證限制。

---

## 驗證計畫

### 自動化測試

目前專案尚未建立單元測試或整合測試框架，因此本次暫不加入自動化測試。

### 手動驗證

#### 1. 執行 Migration 建立資料表

```bash
cd c:\Users\VivoBook\Desktop\betterthanvieshow\betterthanvieshow
dotnet ef database update
```

**驗證點**：
- 確認 Migration 成功執行
- 檢查資料庫中是否成功建立 `Theater` 資料表

#### 2. 使用 Scalar UI 測試 API

1. 啟動應用程式：
   ```bash
   dotnet run
   ```

2. 開啟瀏覽器，前往 Scalar UI：`https://localhost:5001/scalar/v1`

3. 測試案例：

   **測試案例 1：未授權存取（應回傳 401）**
   - 方法：GET
   - 端點：`/api/admin/theaters`
   - 不帶 Authorization Header
   - 預期結果：HTTP 401 Unauthorized

   **測試案例 2：空資料表查詢（應回傳空陣列）**
   - 方法：GET
   - 端點：`/api/admin/theaters`
   - 帶入有效的 Admin JWT Token
   - 預期結果：
     ```json
     {
       "success": true,
       "message": "查詢成功",
       "data": []
     }
     ```

   **測試案例 3：有資料查詢（需先手動新增測試資料）**
   - 在資料庫手動新增一筆影廳測試資料
   - 方法：GET
   - 端點：`/api/admin/theaters`
   - 帶入有效的 Admin JWT Token
   - 預期結果：回傳包含該影廳資料的陣列

**驗證重點**：
- API 回應格式符合 `ApiResponse<List<TheaterResponseDto>>` 結構
- 僅 Admin 角色可存取
- 資料正確對應資料庫內容
