# 刪除影廳 500 錯誤修復 Walkthrough

## 📋 Bug 描述

**API 端點：** `DELETE /api/admin/theaters/{id}`  
**問題：** 當嘗試刪除影廳時，API 返回 500 Internal Server Error  
**發現時間：** 2025-12-29  
**嚴重程度：** 高（影響核心功能且錯誤訊息不明確）

### 錯誤表現

```json
{
  "success": false,
  "message": "刪除影廳失敗，請稍後再試",
  "data": null,
  "errors": null
}
```

**狀態碼：** 500 Internal Server Error

---

## 🔍 問題根源分析

### 1. 資料庫層面

在 `ApplicationDbContext.cs` 中，`MovieShowTime` 與 `Theater` 的外鍵關聯配置為：

```csharp
// 第 165-169 行
entity.HasOne(e => e.Theater)
    .WithMany()
    .HasForeignKey(e => e.TheaterId)
    .OnDelete(DeleteBehavior.Restrict);  // ⚠️ 關鍵設定
```

**`DeleteBehavior.Restrict` 的意義：**
- 當影廳有任何關聯的場次（MovieShowTime）時
- 資料庫會**拒絕刪除**操作
- 拋出 `DbUpdateException` 異常

### 2. 業務邏輯層面

在 `TheaterService.DeleteTheaterAsync` 方法中（第 180-223 行）：

**問題代碼：**
```csharp
public async Task<ApiResponse<object>> DeleteTheaterAsync(int id)
{
    try
    {
        // 檢查影廳是否存在
        var exists = await _theaterRepository.ExistsAsync(id);
        if (!exists)
        {
            return ApiResponse<object>.FailureResponse("找不到指定的影廳");
        }

        // ❌ 缺少檢查：沒有檢查是否有關聯場次
        // TODO 註解提示需要實作，但實際未實作

        // 直接刪除 → 觸發資料庫外鍵約束錯誤
        await _theaterRepository.DeleteAsync(id);
        
        return ApiResponse<object>.SuccessResponse(new object(), "影廳刪除成功");
    }
    catch (Exception ex)  // ⚠️ 捕獲資料庫異常
    {
        _logger.LogError(ex, "刪除影廳時發生錯誤，影廳 ID: {TheaterId}", id);
        return ApiResponse<object>.FailureResponse("刪除影廳失敗，請稍後再試");
        // ❌ 回傳通用 500 錯誤，沒有明確告知原因
    }
}
```

**執行流程：**
1. 檢查影廳是否存在 ✓
2. ❌ **缺少**檢查是否有關聯場次
3. 嘗試刪除影廳
4. 資料庫拋出外鍵約束異常
5. catch 捕獲異常
6. 回傳通用 500 錯誤訊息

---

## ✅ 修復方案

### 核心思路

在刪除影廳前，先檢查是否有關聯的場次：
- **有場次** → 回傳 400 Bad Request，明確提示無法刪除
- **無場次** → 正常刪除，回傳 200 OK

### 修復步驟

#### 步驟 1：擴展 Repository 介面

**檔案：** `Repositories/Interfaces/ITheaterRepository.cs`

**新增方法：**
```csharp
/// <summary>
/// 檢查影廳是否有關聯的場次
/// </summary>
/// <param name="id">影廳 ID</param>
/// <returns>有場次回傳 true，否則回傳 false</returns>
Task<bool> HasShowtimesAsync(int id);
```

#### 步驟 2：實作 Repository 方法

**檔案：** `Repositories/Implementations/TheaterRepository.cs`

**實作邏輯：**
```csharp
/// <summary>
/// 檢查影廳是否有關聯的場次
/// </summary>
/// <param name="id">影廳 ID</param>
/// <returns>有場次回傳 true，否則回傳 false</returns>
public async Task<bool> HasShowtimesAsync(int id)
{
    return await _context.MovieShowTimes.AnyAsync(s => s.TheaterId == id);
}
```

**關鍵邏輯：**
- 使用 `AnyAsync` 查詢 `MovieShowTimes` 表
- 檢查是否存在 `TheaterId == id` 的記錄
- 高效：只檢查存在性，不返回完整資料

#### 步驟 3：更新 Service 業務邏輯

**檔案：** `Services/Implementations/TheaterService.cs`

**修復後的代碼：**
```csharp
public async Task<ApiResponse<object>> DeleteTheaterAsync(int id)
{
    try
    {
        // 檢查影廳是否存在
        var exists = await _theaterRepository.ExistsAsync(id);
        if (!exists)
        {
            return ApiResponse<object>.FailureResponse("找不到指定的影廳");
        }

        // ✅ 新增：檢查是否有關聯的場次
        var hasShowtimes = await _theaterRepository.HasShowtimesAsync(id);
        if (hasShowtimes)
        {
            // ✅ 回傳 400 Bad Request，明確提示原因
            return ApiResponse<object>.FailureResponse("影廳目前有場次安排，無法刪除");
        }

        // 刪除影廳及其座位
        await _theaterRepository.DeleteAsync(id);

        return ApiResponse<object>.SuccessResponse(new object(), "影廳刪除成功");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "刪除影廳時發生錯誤，影廳 ID: {TheaterId}", id);
        return ApiResponse<object>.FailureResponse("刪除影廳失敗，請稍後再試");
    }
}
```

**改進點：**
1. ✅ 在刪除前檢查場次關聯
2. ✅ 有場次時回傳 400 而非 500
3. ✅ 提供清楚的錯誤訊息
4. ✅ 避免觸發資料庫異常

---

## 🧪 測試驗證

### 測試場景 1：刪除有場次的影廳（ID 2）

**請求：**
```http
DELETE /api/admin/theaters/2
Authorization: Bearer {admin_token}
```

**預期回應：**
- **狀態碼：** 400 Bad Request
- **回應內容：**
```json
{
  "success": false,
  "message": "影廳目前有場次安排，無法刪除",
  "data": null,
  "errors": null
}
```

**✅ 測試結果：** 通過

---

### 測試場景 2：刪除沒有場次的影廳（ID 3）

**請求：**
```http
DELETE /api/admin/theaters/3
Authorization: Bearer {admin_token}
```

**預期回應：**
- **狀態碼：** 200 OK
- **回應內容：**
```json
{
  "success": true,
  "message": "影廳刪除成功",
  "data": {},
  "errors": null
}
```

**✅ 測試結果：** 通過，影廳已成功從資料庫中刪除

---

## 📊 修復前後對比

| 項目 | 修復前 | 修復後 |
|------|--------|--------|
| **有場次的影廳** | 500 錯誤，訊息不明確 | 400 錯誤，明確提示原因 |
| **無場次的影廳** | 理論上可刪除，但未測試 | 200 成功刪除 ✓ |
| **錯誤訊息** | "刪除影廳失敗，請稍後再試" | "影廳目前有場次安排，無法刪除" |
| **使用者體驗** | 不知道為什麼失敗 | 清楚知道失敗原因 |
| **資料庫異常** | 觸發外鍵約束錯誤 | 業務層提前檢查，避免異常 |

---

## 📝 相關檔案清單

### 修改的檔案

1. **`Repositories/Interfaces/ITheaterRepository.cs`**
   - 新增 `HasShowtimesAsync` 方法定義

2. **`Repositories/Implementations/TheaterRepository.cs`**
   - 實作 `HasShowtimesAsync` 方法

3. **`Services/Implementations/TheaterService.cs`**
   - 更新 `DeleteTheaterAsync` 方法
   - 移除 TODO 註解，實作場次檢查邏輯

### 相關配置檔案（未修改，僅供參考）

4. **`Data/ApplicationDbContext.cs`**
   - 外鍵約束配置（第 165-169 行）

5. **`Controllers/TheatersController.cs`**
   - API 端點定義（第 217-265 行）

---

## 🎯 學習要點

### 1. 外鍵約束的重要性

- `DeleteBehavior.Restrict` 可防止意外刪除有依賴關係的資料
- 但需要在業務層提前檢查，提供友善的錯誤提示

### 2. 錯誤碼的正確使用

- **400 Bad Request：** 業務邏輯不允許（如：有場次無法刪除）
- **404 Not Found：** 資源不存在
- **500 Internal Server Error：** 系統異常（應盡量避免）

### 3. 防禦性編程

- 在執行破壞性操作（刪除）前，進行完整的前置檢查
- 提供明確的錯誤訊息，讓使用者知道如何處理

### 4. Repository 模式的價值

- 將資料庫查詢邏輯封裝在 Repository 層
- 業務層專注於業務規則，不直接操作 DbContext

---

## ✨ 總結

此次修復成功解決了刪除影廳 API 的 500 錯誤問題：

1. ✅ **問題診斷：** 找出外鍵約束導致的資料庫異常
2. ✅ **方案設計：** 在業務層添加場次檢查邏輯
3. ✅ **代碼實作：** 擴展 Repository，更新 Service
4. ✅ **測試驗證：** 完整測試兩種場景，確認修復有效

**修復後的 API 行為：**
- 有場次的影廳 → 明確回傳 400，提示無法刪除
- 無場次的影廳 → 成功刪除，回傳 200

**用戶體驗提升：**
- 從「不知道為什麼失敗」到「清楚知道失敗原因」
- 錯誤訊息更友善，符合 RESTful API 最佳實踐

---

**修復分支：** `bugfix/delete-theater-api`  
**修復日期：** 2025-12-29  
**修復人員：** Gemini AI & 開發團隊
