# 影廳列表 API 實作計畫

## 目標

開發影廳管理 API 的第一支端點：`GET /api/admin/theaters`，讓管理者可以查詢所有影廳資料。

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

#### [NEW] [TheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TheaterRepository.cs)

實作 `ITheaterRepository`，使用 Entity Framework Core 查詢所有影廳資料。

---

### 服務層 (Service & DTO)

#### [NEW] [TheaterResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/TheaterResponseDto.cs)

定義影廳回應 DTO，包含以下欄位：
- `Id`: 影廳 ID
- `Name`: 影廳名稱
- `Type`: 影廳類型
- `Floor`: 樓層
- `RowCount`: 排數
- `ColumnCount`: 列數
- `TotalSeats`: 座位總數

#### [NEW] [ITheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITheaterService.cs)

定義影廳服務介面，提供 `GetAllTheatersAsync()` 方法，回傳 `ApiResponse<List<TheaterResponseDto>>`。

#### [NEW] [TheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)

實作 `ITheaterService`，負責：
1. 呼叫 Repository 取得所有影廳
2. 將 Entity 轉換為 DTO
3. 包裝成 `ApiResponse` 格式回傳

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
        "totalSeats": 120
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
