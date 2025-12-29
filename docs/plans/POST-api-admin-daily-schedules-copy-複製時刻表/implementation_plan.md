# 複製時刻表 API 實作計畫

## 目標

實作 `POST /api/admin/daily-schedules/{sourceDate}/copy` API，允許管理者將已販售（OnSale）的時刻表複製到草稿（Draft）狀態的日期，用於快速排片。

## 功能需求

根據 [`複製時刻表.feature`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/複製時刻表.feature)，此 API 需滿足以下商業規則：

### Rule 1: 只有販售中（OnSale）的時刻表可以被複製
- ✅ **允許**：複製 `OnSale` 狀態的時刻表
- ❌ **禁止**：複製 `Draft` 狀態的時刻表，應回傳 `400 Bad Request`，錯誤訊息：「只能複製已販售的時刻表」

### Rule 2: 只能複製到草稿（Draft）狀態的日期
- ✅ **允許**：複製到 `Draft` 狀態的日期（覆蓋模式：先刪除目標日期的所有舊場次，再複製新場次）
- ❌ **禁止**：複製到 `OnSale` 狀態的日期，應回傳 `400 Bad Request`，錯誤訊息：「目標日期必須為草稿狀態」

### Rule 3: 自動略過檔期不符的場次
- 檢查每個場次的電影在目標日期是否仍在檔期內（`releaseDate <= targetDate <= endDate`）
- 只複製檔期內的場次，自動略過已下映的電影場次
- 如有場次被略過，回傳提示訊息：「部分場次因電影檔期已過期未複製」

---

## 實作變更

### DTO 層

#### [NEW] [CopyDailyScheduleRequestDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CopyDailyScheduleRequestDto.cs)

```csharp
/// <summary>
/// 複製時刻表請求 DTO
/// </summary>
public class CopyDailyScheduleRequestDto
{
    /// <summary>
    /// 目標日期（複製到此日期），格式：YYYY-MM-DD
    /// </summary>
    [Required(ErrorMessage = "目標日期為必填")]
    public string TargetDate { get; set; } = string.Empty;
}
```

#### [NEW] [CopyDailyScheduleResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/CopyDailyScheduleResponseDto.cs)

```csharp
/// <summary>
/// 複製時刻表回應 DTO
/// </summary>
public class CopyDailyScheduleResponseDto
{
    /// <summary>
    /// 來源日期
    /// </summary>
    public DateTime SourceDate { get; set; }
    
    /// <summary>
    /// 目標日期
    /// </summary>
    public DateTime TargetDate { get; set; }
    
    /// <summary>
    /// 成功複製的場次數量
    /// </summary>
    public int CopiedCount { get; set; }
    
    /// <summary>
    /// 被略過的場次數量（因電影檔期問題）
    /// </summary>
    public int SkippedCount { get; set; }
    
    /// <summary>
    /// 提示訊息（如：部分場次因電影檔期已過期未複製）
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// 目標時刻表資訊
    /// </summary>
    public DailyScheduleResponseDto TargetSchedule { get; set; } = null!;
}
```

---

### Repository 層

#### [MODIFY] [IShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Interfaces/IShowtimeRepository.cs)

新增方法：

```csharp
/// <summary>
/// 取得指定日期的所有場次（包含電影資訊，用於複製時檢查檔期）
/// </summary>
/// <param name="showDate">放映日期</param>
Task<List<MovieShowTime>> GetByDateWithMovieAsync(DateTime showDate);
```

#### [MODIFY] [ShowtimeRepository.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Repositories/Implementations/ShowtimeRepository.cs)

實作 `GetByDateWithMovieAsync` 方法：

```csharp
public async Task<List<MovieShowTime>> GetByDateWithMovieAsync(DateTime showDate)
{
    return await _context.MovieShowTimes
        .Include(s => s.Movie)
        .Include(s => s.Theater)
        .Where(s => s.ShowDate.Date == showDate.Date)
        .OrderBy(s => s.StartTime)
        .ToListAsync();
}
```

---

### Service 層

#### [MODIFY] [IDailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IDailyScheduleService.cs)

新增方法：

```csharp
/// <summary>
/// 複製時刻表
/// </summary>
/// <param name="sourceDate">來源日期</param>
/// <param name="dto">複製請求</param>
/// <returns>複製結果</returns>
Task<CopyDailyScheduleResponseDto> CopyDailyScheduleAsync(DateTime sourceDate, CopyDailyScheduleRequestDto dto);
```

#### [MODIFY] [DailyScheduleService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/DailyScheduleService.cs)

實作 `CopyDailyScheduleAsync` 方法，包含以下核心邏輯：

1. **驗證來源時刻表**：
   - 檢查來源日期的時刻表是否存在
   - 檢查來源時刻表狀態是否為 `OnSale`（如果不是，拋出 `ArgumentException`：「只能複製已販售的時刻表」）

2. **驗證/建立目標時刻表**：
   - 檢查目標日期的時刻表狀態
   - 如果目標時刻表存在且狀態為 `OnSale`，拋出 `ArgumentException`：「目標日期必須為草稿狀態」
   - 如果目標時刻表不存在，建立新的 `Draft` 狀態時刻表

3. **刪除目標日期的舊場次**（覆蓋模式）：
   - 使用 `ShowtimeRepository.DeleteByDateAsync` 刪除目標日期的所有舊場次

4. **複製場次（含檔期檢查）**：
   - 使用 `ShowtimeRepository.GetByDateWithMovieAsync` 取得來源日期的所有場次（包含電影資訊）
   - 遍歷每個場次，檢查電影在目標日期是否仍在檔期內：
     - 檢查條件：`movie.ReleaseDate <= targetDate && targetDate <= movie.EndDate`
     - 符合條件：加入待複製清單
     - 不符合條件：計入 `skippedCount`
   - 使用 `ShowtimeRepository.CreateBatchAsync` 批次建立新場次

5. **回傳結果**：
   - 建立 `CopyDailyScheduleResponseDto`，包含：
     - 來源日期、目標日期
     - 成功複製數量、略過數量
     - 提示訊息（如有場次被略過）
     - 目標時刻表完整資訊

---

### Controller 層

#### [MODIFY] [DailySchedulesController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/DailySchedulesController.cs)

新增 API 端點：

```csharp
/// <summary>
/// /api/admin/daily-schedules/{sourceDate}/copy 複製時刻表
/// </summary>
/// <remarks>
/// 將指定來源日期的時刻表複製到目標日期，用於快速排片。
/// 
/// **商業規則**：
/// 1. 只能複製 OnSale（販售中）狀態的時刻表
/// 2. 只能複製到 Draft（草稿）狀態的日期
/// 3. 覆蓋模式：會先刪除目標日期的所有舊場次，再複製新場次
/// 4. 自動略過檔期不符的場次（電影已下映）
/// 
/// **範例請求**：
/// ```json
/// {
///   "targetDate": "2025-12-25"
/// }
/// ```
/// 
/// **範例回應**：
/// ```json
/// {
///   "sourceDate": "2025-12-22",
///   "targetDate": "2025-12-25",
///   "copiedCount": 8,
///   "skippedCount": 2,
///   "message": "部分場次因電影檔期已過期未複製",
///   "targetSchedule": { ... }
/// }
/// ```
/// </remarks>
/// <param name="sourceDate">來源日期，格式：YYYY-MM-DD</param>
/// <param name="request">複製請求</param>
/// <response code="200">複製成功</response>
/// <response code="400">參數錯誤（來源時刻表狀態不是 OnSale、目標時刻表狀態不是 Draft 等）</response>
/// <response code="404">來源日期沒有時刻表記錄</response>
/// <response code="401">未授權</response>
[HttpPost("{sourceDate}/copy")]
[ProducesResponseType(typeof(CopyDailyScheduleResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> CopyDailySchedule(
    [FromRoute] string sourceDate,
    [FromBody] CopyDailyScheduleRequestDto request)
{
    try
    {
        // 解析來源日期
        if (!DateTime.TryParse(sourceDate, out var parsedSourceDate))
        {
            return BadRequest(new { message = "來源日期格式錯誤，必須為 YYYY-MM-DD" });
        }

        var result = await _dailyScheduleService.CopyDailyScheduleAsync(parsedSourceDate, request);
        return Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

---

## 驗證計畫

### 自動化測試

建立測試檔案：`docs/tests/複製時刻表API/test-copy-daily-schedule.http`

測試情境包括：

1. **成功複製販售中的時刻表**
   - 前置條件：建立來源日期（OnSale）和目標日期（Draft）的時刻表
   - 預期：200 OK，成功複製所有場次

2. **禁止複製草稿狀態的時刻表**
   - 前置條件：來源日期狀態為 Draft
   - 預期：400 Bad Request，錯誤訊息「只能複製已販售的時刻表」

3. **禁止複製到已販售的日期**
   - 前置條件：目標日期狀態為 OnSale
   - 預期：400 Bad Request，錯誤訊息「目標日期必須為草稿狀態」

4. **覆蓋模式測試**
   - 前置條件：目標日期已有部分場次
   - 預期：200 OK，舊場次被刪除，新場次成功複製

5. **檔期檢查測試**
   - 前置條件：來源日期有兩部電影的場次，其中一部在目標日期已下映
   - 預期：200 OK，只複製檔期內的電影場次，`skippedCount > 0`，回傳提示訊息

6. **日期格式錯誤**
   - 預期：400 Bad Request

7. **來源日期不存在**
   - 預期：404 Not Found

### 手動測試

使用 Scalar API 文件介面進行測試：
1. 登入取得管理員 Token
2. 使用 `POST /api/admin/daily-schedules/{sourceDate}/copy` 端點
3. 驗證各種情境下的回應是否符合預期
