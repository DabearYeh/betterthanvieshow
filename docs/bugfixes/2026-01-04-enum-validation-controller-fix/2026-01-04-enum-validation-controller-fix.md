# Bug 修復：Enum 驗證錯誤處理

## 問題編號
內部發現 - 2026-01-04

## 問題描述

前端在呼叫 `POST /api/admin/theaters` API 時，如果傳入中文的影廳類型（如 `"一般數位"`），API 會回傳 **500 Internal Server Error** 而非 **400 Bad Request**。

這導致前端無法正確處理驗證錯誤，且錯誤訊息不明確。

## 重現步驟

1. 使用 Admin Token 呼叫 `POST /api/admin/theaters`
2. 請求 Body 包含：`{"type": "一般數位", ...}`
3. API 回傳 500 錯誤

**預期行為**：應回傳 400 Bad Request 並包含清楚的錯誤訊息

## 根本原因

### 問題 1：Service 層缺少驗證
`TheaterService.CreateTheaterAsync()` 和 `MovieService` 的相關方法**沒有驗證** Enum 值，導致錯誤數據可以存入資料庫。

### 問題 2：Controller 錯誤處理不完整
即使 Service 回傳失敗，Controller 的錯誤處理邏輯**沒有涵蓋**新加入的驗證錯誤訊息，導致這些錯誤被歸類為 500 錯誤。

**錯誤處理邏輯問題**：
```csharp
// TheatersController.cs - 原本的邏輯
if (result.Message?.Contains("座位陣列") == true || 
    result.Message?.Contains("影廳必須") == true ||
    result.Message?.Contains("不符") == true)  // ❌ 沒有包含 "影廳類型無效"
{
    return BadRequest(result);
}
return StatusCode(500, result);  // 導致驗證錯誤被當成 500
```

## 修復方案

### 1. Service 層加入驗證邏輯

#### TheaterService.cs
在 `CreateTheaterAsync()` 開頭加入：

```csharp
// 驗證影廳類型
var allowedTypes = new[] { "Digital", "IMAX", "4DX" };
if (!allowedTypes.Contains(request.Type))
{
    _logger.LogWarning("建立影廳失敗: 無效的影廳類型 '{Type}'", request.Type);
    return ApiResponse<TheaterResponseDto>.FailureResponse(
        $"影廳類型無效: {request.Type}。允許的值: {string.Join(", ", allowedTypes)}"
    );
}

// 驗證座位類型
var allowedSeatTypes = new[] { "Standard", "Wheelchair", "Aisle", "Empty" };
for (int row = 0; row < request.RowCount; row++)
{
    for (int col = 0; col < request.ColumnCount; col++)
    {
        string seatType = request.Seats[row][col];
        if (!allowedSeatTypes.Contains(seatType))
        {
            return ApiResponse<TheaterResponseDto>.FailureResponse(
                $"無效的座位類型 '{seatType}' 於位置 (Row {row + 1}, Col {col + 1})。允許的值: {string.Join(", ", allowedSeatTypes)}"
            );
        }
    }
}
```

#### MovieService.cs
在 `CreateMovieAsync()` 和 `UpdateMovieAsync()` 中加入：

```csharp
// Rating 驗證
var allowedRatings = new[] { "G", "P", "PG", "R" };
if (!allowedRatings.Contains(request.Rating))
{
    return ApiResponse<MovieResponseDto>.FailureResponse(
        $"電影分級無效: {request.Rating}。允許的值: {string.Join(", ", allowedRatings)}"
    );
}

// Genre 驗證
var allowedGenres = new[] { "Action", "Romance", "Adventure", "Thriller", "Horror", "SciFi", "Animation", "Comedy" };
var genres = request.Genre.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var invalidGenres = genres.Where(g => !allowedGenres.Contains(g)).ToList();

if (invalidGenres.Any())
{
    return ApiResponse<MovieResponseDto>.FailureResponse(
        $"無效的電影類型: {string.Join(", ", invalidGenres)}。允許的值: {string.Join(", ", allowedGenres)}"
    );
}
```

### 2. Controller 層修正錯誤處理

#### TheatersController.cs
```csharp
if (!result.Success)
{
    if (result.Message?.Contains("座位陣列") == true || 
        result.Message?.Contains("影廳必須") == true ||
        result.Message?.Contains("不符") == true ||
        result.Message?.Contains("影廳類型無效") == true ||  // ✅ 新增
        result.Message?.Contains("座位類型") == true ||      // ✅ 新增
        result.Message?.Contains("無效") == true)            // ✅ 新增
    {
        return BadRequest(result);
    }
    return StatusCode(500, result);
}
```

#### MoviesController.cs
```csharp
if (!result.Success)
{
    if (result.Message?.Contains("日期") == true ||
        result.Message?.Contains("分級無效") == true ||  // ✅ 新增
        result.Message?.Contains("電影類型") == true ||  // ✅ 新增
        result.Message?.Contains("無效") == true)        // ✅ 新增
    {
        return BadRequest(result);
    }
    return StatusCode(500, result);
}
```

## 修改的檔案

1. [`TheaterService.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/TheaterService.cs)
2. [`MovieService.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/MovieService.cs)
3. [`TheatersController.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/TheatersController.cs)
4. [`MoviesController.cs`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

## 測試驗證

### 測試結果：✅ 4/4 全部通過

| 測試項目 | 輸入值 | 預期 | 實際 | 狀態 |
|---------|--------|------|------|------|
| Theater Type | `"一般數位"` | 400 | 400 | ✅ |
| Seat Type | `"普通","走道"` | 400 | 400 | ✅ |
| Movie Rating | `"普遍級"` | 400 | 400 | ✅ |
| Movie Genre | `"動作,科幻"` | 400 | 400 | ✅ |

**測試詳情**：[enum_validation_test_results.md](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/bugfixes/2026-01-04-enum-validation-controller-fix/enum_validation_test_results.md)

## 測試檔案

本次修復包含以下測試檔案：

1. **[enum_validation_theater.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/bugfixes/2026-01-04-enum-validation-controller-fix/enum_validation_theater.http)** - Theater API 測試案例 (REST Client)
2. **[enum_validation_movie.http](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/bugfixes/2026-01-04-enum-validation-controller-fix/enum_validation_movie.http)** - Movie API 測試案例 (REST Client)
3. **[test_theater_enum_validation.ps1](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/bugfixes/2026-01-04-enum-validation-controller-fix/test_theater_enum_validation.ps1)** - PowerShell 測試腳本
4. **[enum_validation_test_results.md](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/bugfixes/2026-01-04-enum-validation-controller-fix/enum_validation_test_results.md)** - 完整測試結果報告


## 影響範圍

- ✅ 不影響現有正常的 API 呼叫
- ✅ 只影響輸入錯誤值的情況（原本就該失敗）
- ✅ 改善錯誤訊息的清晰度
- ⚠️ 前端需要更新，使用英文 Enum 值

## 後續行動

1. ✅ 實作驗證邏輯
2. ✅ 測試驗證
3. [ ] 通知前端工程師使用英文 Enum 值
4. [ ] 檢查資料庫是否有舊的中文值需要清理

## 相關文件

- [API Enum 驗證報告](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/reports/API_Enum_Validation_Report.md)
- [實作 Walkthrough](file:///C:/Users/VivoBook/.gemini/antigravity/brain/79249e34-f6f8-4645-ab92-114242dce573/walkthrough.md)
