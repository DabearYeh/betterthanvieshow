# DELETE /api/admin/theaters/{id} API 實作計劃

## 需求說明

根據用戶澄清，刪除規則如下：

### 刪除規則
1. **影廳**：只有在還沒被使用之前可以刪除（即沒有在時刻表上被放電影 / 沒有關聯的場次）
2. **電影**：不能刪除
3. **場次**：不能刪除

### 業務邏輯
- 檢查影廳是否有關聯的場次（`MovieShowTime`）
- 如果有關聯場次，回傳錯誤訊息，禁止刪除
- 如果沒有關聯場次，允許刪除影廳及其相關座位

### 資料庫關聯（根據 ERM）
根據 [`erm.dbml`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/erm.dbml)：
- `Theater` 1:N `Seat` （影廳有多個座位）
- `Theater` 1:N `MovieShowTime` （影廳有多個場次）

刪除影廳時需要：
1. 檢查是否有關聯的 `MovieShowTime`
2. 如果沒有場次，同時刪除關聯的 `Seat` 記錄
3. 最後刪除 `Theater` 記錄

## User Review Required

> [!WARNING]
> **目前狀態**
> 
> 根據程式碼檢查，系統目前還沒有實作 `MovieShowTime` (場次) 實體類別。
> 
> **實作方案：**
> 
> 由於場次功能尚未實作，我建議採用以下階段性方案：
> 
> 1. **現階段**：實作基本的刪除影廳功能
>    - 檢查影廳是否存在
>    - 刪除關聯的座位（`Seat`）
>    - 刪除影廳本身
>    - 為未來的場次檢查預留介面
> 
> 2. **後續階段**：當 `MovieShowTime` 實體建立後
>    - 在刪除前檢查是否有關聯場次
>    - 如果有場次，拒絕刪除並回傳錯誤
> 
> 這樣可以確保：
> - 現在就能使用刪除影廳功能（測試和開發）
> - 未來擴展時只需添加場次檢查邏輯，不需要大改架構

## Proposed Changes

### Repository 層

#### [MODIFY] [ITheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/ITheaterRepository.cs)

新增方法：
```csharp
/// <summary>
/// 檢查影廳是否存在
/// </summary>
Task<bool> ExistsAsync(int id);

/// <summary>
/// 刪除影廳及其所有座位
/// </summary>
Task DeleteAsync(int id);
```

#### [MODIFY] [TheaterRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/TheaterRepository.cs)

實作新增的方法：
- `ExistsAsync`: 檢查影廳是否存在
- `DeleteAsync`: 
  - 刪除所有關聯的座位（`Seat`）
  - 刪除影廳記錄
  - 使用 Transaction 確保資料一致性

---

### Service 層

#### [MODIFY] [ITheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/ITheaterService.cs)

新增方法：
```csharp
/// <summary>
/// 刪除影廳
/// </summary>
Task<ApiResponse<object>> DeleteTheaterAsync(int id);
```

#### [MODIFY] [TheaterService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)

實作刪除邏輯：
1. 檢查影廳是否存在，不存在回傳 `404 Not Found`
2. **TODO（未來實作）**: 檢查是否有關聯的場次（`MovieShowTime`）
   - 如果有場次，回傳錯誤：「影廳目前有場次安排，無法刪除」
3. 呼叫 Repository 層刪除影廳
4. 回傳成功訊息

---

### Controller 層

#### [MODIFY] [TheatersController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TheatersController.cs)

新增 `DeleteTheater` 端點：
- HTTP Method: `DELETE`
- Route: `/api/admin/theaters/{id}`
- Authorization: `Admin` 角色
- 回應狀態碼：
  - `200 OK`: 刪除成功
  - `404 Not Found`: 影廳不存在
  - `400 Bad Request`: 影廳有關聯場次（未來實作）
  - `401 Unauthorized`: 未登入
  - `403 Forbidden`: 非 Admin 角色
  - `500 Internal Server Error`: 伺服器錯誤

---

## Verification Plan

### Manual Verification

使用 Swagger UI 或 Postman 測試：

#### 1. 成功刪除影廳
```http
DELETE /api/admin/theaters/1
Authorization: Bearer <admin_token>
```
**預期結果**:
- Status: `200 OK`
- Response Body:
```json
{
  "success": true,
  "message": "影廳刪除成功",
  "data": null,
  "errors": null
}
```

#### 2. 刪除不存在的影廳
```http
DELETE /api/admin/theaters/999999
Authorization: Bearer <admin_token>
```
**預期結果**:
- Status: `404 Not Found`
- Response Body:
```json
{
  "success": false,
  "message": "找不到指定的影廳",
  "data": null,
  "errors": null
}
```

#### 3. 未授權訪問
```http
DELETE /api/admin/theaters/1
```
**預期結果**:
- Status: `401 Unauthorized`

#### 4. 非 Admin 角色訪問
```http
DELETE /api/admin/theaters/1
Authorization: Bearer <customer_token>
```
**預期結果**:
- Status: `403 Forbidden`

#### 5. **（未來測試）** 刪除有場次的影廳
當 `MovieShowTime` 實作後：
```http
DELETE /api/admin/theaters/1
Authorization: Bearer <admin_token>
```
**預期結果**:
- Status: `400 Bad Request`
- Response Body:
```json
{
  "success": false,
  "message": "影廳目前有場次安排，無法刪除",
  "data": null,
  "errors": null
}
```

### Database Verification

刪除影廳後，檢查資料庫：
1. `Theater` 表中該影廳記錄已被刪除
2. `Seat` 表中該影廳相關的所有座位已被刪除

