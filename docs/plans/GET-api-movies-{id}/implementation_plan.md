# 實作前台電影詳情 API

本計畫將實作前台電影詳情功能，讓用戶可以查看單一電影的完整資訊。

## 背景說明

根據前端 UI 設計（如圖所示），用戶點擊電影後會進入詳情頁面，顯示：
- 電影海報與預告片
- 標題、分級、時長
- 類型標籤
- 劇情介紹
- 導演、演員
- 上映日期與下映日期

**現有架構分析**：
- ✅ Repository 層已有 `GetByIdAsync(int id)` 方法
- ✅ Service 層已有 `GetMovieByIdAsync(int id)` 方法
- ✅ DTO 層已有 `MovieResponseDto`（包含所有需要的欄位）
- ❌ Controller 層只有 Admin 端點 `GET /api/admin/movies/{id}`（需要授權）

**解決方案**：
在同一個 `MoviesController` 中添加公開端點，重用現有的 Service 方法。

![前台電影詳情UI](file:///C:/Users/VivoBook/.gemini/antigravity/brain/6081f386-445b-48fc-8109-e1a762cd483a/uploaded_image_1766557526539.png)

---

## API 設計

**端點路徑**：`GET /api/movies/{id}`

**路由差異**：
- Admin 端點：`GET /api/admin/movies/{id}` - 需要 Admin 授權
- 前台端點：`GET /api/movies/{id}` - 公開存取，無需授權

**參數**：
- `id` (必填) - 電影 ID

**範例**：
```
GET /api/movies/1
```

---

## 提議的變更

### Controller 層

#### [MODIFY] [MoviesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/MoviesController.cs)

在現有的 `GetMovieById` (Admin endpoint) 之後，新增公開端點：

```csharp
/// <summary>
/// 取得電影詳情（前台）
/// </summary>
/// <remarks>
/// 取得單一電影的完整資訊，包括：
/// - 基本資訊：標題、分級、時長、類型
/// - 詳細資訊：劇情介紹、導演、演員
/// - 媒體：海報、預告片連結
/// - 上映時間：上映日期、下映日期
/// 
/// **無需授權**，任何使用者皆可存取。
/// </remarks>
/// <param name="id">電影 ID</param>
/// <response code="200">成功取得電影詳情</response>
/// <response code="404">找不到指定的電影</response>
/// <response code="500">伺服器內部錯誤</response>
/// <returns>電影詳情</returns>
[HttpGet("~/api/movies/{id}")]
[AllowAnonymous]
[ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<MovieResponseDto>), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetMovieDetailForFrontend(int id)
{
    var result = await _movieService.GetMovieByIdAsync(id);

    if (!result.Success)
    {
        if (result.Message?.Contains("找不到") == true)
        {
            return NotFound(result);
        }
        return StatusCode(500, result);
    }

    return Ok(result);
}
```

> [!NOTE]
> **重用現有邏輯**
> 
> 此端點直接重用 `_movieService.GetMovieByIdAsync(id)` 方法，無需修改 Service 或 Repository 層。
> 唯一差異是路由和授權要求。

---

## 驗證計畫

### HTTP 測試

創建測試檔案：`docs/plans/GET-frontend-movie-detail/tests/get-movie-detail.http`

**測試場景：**

1. **成功取得電影詳情**
   ```http
   GET {{baseUrl}}/api/movies/1
   ```
   - 預期：200 OK，返回完整電影資訊

2. **電影不存在**
   ```http
   GET {{baseUrl}}/api/movies/999999
   ```
   - 預期：404 Not Found

3. **無效的 ID**
   ```http
   GET {{baseUrl}}/api/movies/abc
   ```
   - 預期：400 Bad Request（路由綁定錯誤）

### 手動測試

使用 Scalar UI 測試端點：
- 訪問 `http://localhost:5041/scalar/v1`
- 找到 `GET /api/movies/{id}` 端點
- 輸入有效的電影 ID
- 驗證返回的資料結構

---

## 總結

此實作非常簡單，因為：
- ✅ 不需要新增 DTO
- ✅ 不需要修改 Service 層
- ✅ 不需要修改 Repository 層
- ✅ 只需在 Controller 新增一個公開端點

估計實作時間：**5 分鐘**
