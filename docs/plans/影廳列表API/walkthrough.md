# 影廳列表 API 開發完成

## 實作摘要

已成功建立影廳管理 API 的第一支端點 `GET /api/admin/theaters`，此端點可讓管理者查詢所有影廳資料，並已完成以下實作：

### 已完成的變更

#### 資料層 (Data & Repository)

##### Theater.cs
建立了 `Theater` 實體模型，包含所有規格要求的欄位與驗證規則。

**位置**: [Theater.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/Entities/Theater.cs)

**主要欄位**:
- `Id`: 影廳 ID (主鍵)
- `Name`: 影廳名稱
- `Type`: 影廳類型 (一般數位/4DX/IMAX)
- `Floor`: 所在樓層
- `RowCount`: 排數 (必須 > 0)
- `ColumnCount`: 列數 (必須 > 0)
- `TotalSeats`: 座位總數 (必須 > 0)

##### ITheaterRepository.cs & TheaterRepository.cs
實作了影廳資料存取層，提供 `GetAllAsync()` 方法查詢所有影廳，並依照樓層和名稱排序。

**檔案位置**:
- [ITheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITheaterRepository.cs)
- [TheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TheaterRepository.cs)

---

#### 服務層 (Service & DTO)

##### TheaterResponseDto.cs
定義 API 回應的資料傳輸物件 (DTO)，包含影廳的所有基本資訊。

**檔案位置**: [TheaterResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/TheaterResponseDto.cs)

##### ITheaterService.cs & TheaterService.cs
實作了影廳業務邏輯層，負責：
- 呼叫 Repository 取得資料
- 將 Entity 轉換為 DTO
- 包裝成標準 API 回應格式
- 錯誤處理與日誌記錄

**檔案位置**:
- [ITheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITheaterService.cs)
- [TheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)

---

#### 控制器層 (Controller)

##### TheatersController.cs
建立了影廳管理控制器，實作 `GET /api/admin/theaters` 端點。

**檔案位置**: [TheatersController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TheatersController.cs)

**端點特性**:
- **路由**: `/api/admin/theaters`
- **HTTP 方法**: GET
- **授權**: 需要 Admin 角色
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

#### 資料庫配置

##### ApplicationDbContext.cs
更新了資料庫上下文，加入：
- `Theaters` DbSet
- Theater 實體的資料庫約束配置

**檔案位置**: [ApplicationDbContext.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Data/ApplicationDbContext.cs)

**資料庫約束**:
- `CHK_Theater_RowCount`: 排數 > 0
- `CHK_Theater_ColumnCount`: 列數 > 0
- `CHK_Theater_TotalSeats`: 座位總數 > 0

##### Migration
已建立 `CreateTheaterTable` Migration，準備用於資料庫更新。

---

#### 依賴注入 (Program.cs)
在 [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs) 中註冊了：
- `ITheaterRepository` → `TheaterRepository`
- `ITheaterService` → `TheaterService`

---

## 驗證結果

### 編譯測試

✅ **程式碼編譯成功**

```bash
dotnet build
```

**結果**: 成功建置，無編譯錯誤。

---

## 後續測試步驟

由於本機資料庫連線配置尚未設定（appsettings.json 已被 gitignore 排除），需要進行以下步驟才能完整測試 API：

### 1. 設定資料庫連線

在 `appsettings.Development.json` 或 `appsettings.json` 中加入 Azure SQL 連線字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "您的 Azure SQL 連線字串"
  }
}
```

### 2. 執行 Migration

```bash
cd c:\Users\VivoBook\Desktop\betterthanvieshow\betterthanvieshow
dotnet ef database update
```

此命令將在資料庫中建立 `Theater` 資料表。

### 3. 啟動應用程式

```bash
dotnet run
```

### 4. 使用 Scalar UI 測試 API

1. 開啟瀏覽器前往: `https://localhost:5001/scalar/v1`
2. 找到 `GET /api/admin/theaters` 端點
3. 點擊「Authorize」輸入 Admin 使用者的 JWT Token
4. 執行請求並驗證回應

**測試案例**:
- **未授權存取**: 不帶 Token → 預期 401 Unauthorized
- **Customer 角色**: 使用 Customer Token → 預期 403 Forbidden
- **Admin 角色**: 使用 Admin Token → 預期 200 OK 並回傳影廳列表

---

## 專案檔案結構

```
betterthanvieshow/
├── Controllers/
│   └── TheatersController.cs          ✨ 新增
├── Models/
│   ├── DTOs/
│   │   └── TheaterResponseDto.cs      ✨ 新增
│   └── Entities/
│       └── Theater.cs                  ✨ 新增
├── Repositories/
│   ├── Interfaces/
│   │   └── ITheaterRepository.cs      ✨ 新增
│   └── Implementations/
│       └── TheaterRepository.cs        ✨ 新增
├── Services/
│   ├── Interfaces/
│   │   └── ITheaterService.cs         ✨ 新增
│   └── Implementations/
│       └── TheaterService.cs           ✨ 新增
├── Data/
│   └── ApplicationDbContext.cs         🔧 修改
├── Migrations/
│   └── xxxx_CreateTheaterTable.cs      ✨ 新增
└── Program.cs                          🔧 修改
```

---

## 技術重點

### 分層架構
遵循專案既有的分層架構模式：
1. **Entity**: 資料庫實體定義
2. **Repository**: 資料存取邏輯
3. **Service**: 業務邏輯與 DTO 轉換
4. **Controller**: API 端點與請求處理

### 授權機制
- 使用 `[Authorize(Roles = "Admin")]` 限制只有管理員可存取
- 整合現有的 JWT 認證機制

### API 回應格式
- 統一使用 `ApiResponse<T>` 包裝回應
- 包含 `Success`、`Message`、`Data` 三個欄位
- 錯誤處理包含日誌記錄

### 資料庫約束
- 使用 Entity Framework 的 Check Constraints
- 確保資料完整性（排數、列數、座位總數必須 > 0）

---

## 驗證結果

### ✅ 資料庫 Migration 成功

執行 `dotnet ef database update` 成功建立 Theater 資料表。

### ✅ 應用程式啟動成功

應用程式正常啟動並運行在 `http://localhost:5041`。

### ✅ API 端點在 Scalar UI 顯示

成功在 Scalar API 文件介面看到 `GET /api/admin/Theaters` 端點：

![Scalar UI 顯示 Theaters 端點](/c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/影廳列表API/theaters_endpoint.png)


**端點資訊**:
- **路由**: `/api/admin/Theaters`
- **方法**: GET
- **授權**: 需要 Admin 角色
- **群組**: Theaters
- **狀態**: ✅ 成功顯示在 API 文件中

### 後續測試步驟

要完整測試 API 功能，您需要：

1. **取得 Admin JWT Token**
   - 使用現有的 `/api/Auth/register` 註冊一個 Admin 帳號
   - 或使用 `/api/Auth/login` 登入已有的 Admin 帳號取得 Token

2. **在 Scalar UI 中授權**
   - 點擊右上角的「Authorize」按鈕
   - 輸入 Admin 的 JWT Token

3. **測試 GET /api/admin/Theaters 端點**
   - 展開 Theaters 群組下的 GET 端點
   - 點擊「Send」按鈕發送請求
   - 驗證回應格式符合預期

4. **新增測試資料**（可選）
   - 若資料庫目前沒有影廳資料，可以手動在資料庫新增測試資料
   - 或等待實作 POST /api/admin/theaters (新增影廳) API 後進行測試
