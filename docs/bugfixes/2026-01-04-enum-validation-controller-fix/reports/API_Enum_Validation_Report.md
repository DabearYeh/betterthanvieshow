# API Enum 驗證缺失檢查報告

## 檢查日期
2026-01-04

## 摘要
本報告列出專案中所有缺少 Enum 值驗證的 API 端點。這些端點允許前端輸入任意字串,可能導致資料庫存入不符規範的值,進而在其他業務邏輯中引發錯誤。

---

## 1. ❌ Theater Type (影廳類型)

### 允許的值
- `Digital` (數位廳, 300元)
- `IMAX` (IMAX廳, 380元)
- `4DX` (4DX廳, 380元)

### 缺少驗證的 API

#### 1.1 `POST /api/admin/theaters` - 建立影廳
**檔案**: `Services/Implementations/TheaterService.cs` - `CreateTheaterAsync()`
**問題**: 
- 無驗證邏輯,前端可輸入任意值 (如 `"一般數位"`)
- 導致資料庫存入非英文 Enum 值
- 訂票時 `OrderService.CalculateTicketPrice()` 會拋出異常

**建議修正**:
```csharp
// 在 CreateTheaterAsync 開頭加入
var allowedTypes = new[] { "Digital", "IMAX", "4DX" };
if (!allowedTypes.Contains(request.Type))
{
    return ApiResponse<TheaterResponseDto>.FailureResponse(
        $"影廳類型無效。允許的值: {string.Join(", ", allowedTypes)}"
    );
}
```

**影響範圍**:
- 建立影廳時允許錯誤數據
- 訂票流程會失敗 (`OrderService.CalculateTicketPrice`)
- 票價計算會失敗 (`MovieService.GetPriceByTheaterType`)

---

## 2. ❌ Movie Rating (電影分級)

### 允許的值
- `G` (普遍級)
- `P` (保護級)
- `PG` (輔導級)
- `R` (限制級)

### 缺少驗證的 API

#### 2.1 `POST /api/admin/movies` - 建立電影
**檔案**: `Services/Implementations/MovieService.cs` - `CreateMovieAsync()`
**問題**: 無驗證 `Rating` 欄位,可輸入任意字串

**建議修正**:
```csharp
var allowedRatings = new[] { "G", "P", "PG", "R" };
if (!allowedRatings.Contains(request.Rating))
{
    return ApiResponse<MovieResponseDto>.FailureResponse(
        $"電影分級無效。允許的值: {string.Join(", ", allowedRatings)}"
    );
}
```

#### 2.2 `PUT /api/admin/movies/{id}` - 更新電影
**檔案**: `Services/Implementations/MovieService.cs` - `UpdateMovieAsync()`
**問題**: 同上

---

## 3. ❌ Movie Genre (電影類型)

### 允許的值
- `Action`, `Romance`, `Adventure`, `Thriller`, `Horror`, `SciFi`, `Animation`, `Comedy`

### 缺少驗證的 API

#### 3.1 `POST /api/admin/movies` - 建立電影
**檔案**: `Services/Implementations/MovieService.cs` - `CreateMovieAsync()`
**問題**: 
- `Genre` 欄位為逗號分隔的字串,無驗證
- 可輸入任意值如 `"動作,愛情"`

**建議修正**:
```csharp
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

#### 3.2 `PUT /api/admin/movies/{id}` - 更新電影
**問題**: 同上

---

## 4. ✅ Seat Type (座位類型)

### 允許的值
- `Standard` (一般座位)
- `Wheelchair` (無障礙座位)
- `Aisle` (走道)
- `Empty` (空位)

### API 狀態

#### 4.1 `POST /api/admin/theaters` - 建立影廳
**檔案**: `Services/Implementations/TheaterService.cs` - `CreateTheaterAsync()`
**狀態**: ⚠️ **部分驗證**
- 有驗證 `Standard` 和 `Wheelchair` (計算總座位數時)
- **缺少**對 `Aisle` 和 `Empty` 的驗證
- 可能輸入錯誤值如 `"通道"`, `"普通"` 等

**建議加強驗證**:
```csharp
var allowedSeatTypes = new[] { "Standard", "Wheelchair", "Aisle", "Empty" };
for (int row = 0; row < request.RowCount; row++)
{
    for (int col = 0; col < request.ColumnCount; col++)
    {
        string seatType = request.Seats[row][col];
        if (!allowedSeatTypes.Contains(seatType))
        {
            return ApiResponse<TheaterResponseDto>.FailureResponse(
                $"無效的座位類型 '{seatType}' 於位置 ({row}, {col})。允許的值: {string.Join(", ", allowedSeatTypes)}"
            );
        }
    }
}
```

---

## 5. ✅ Status 欄位 (已由系統控制)

以下 Status 欄位由系統自動設定,**不需要**額外驗證:

### 5.1 DailySchedule.Status
- `Draft` (草稿) - 系統自動設定
- `OnSale` (販售中) - 由 PublishDailySchedule API 設定
- `Closed` (已關閉) - 未使用

### 5.2 Order.Status
- `Pending` (待付款) - 建立訂單時自動設定
- `Paid` (已付款) - 付款成功後設定
- `Cancelled` (已取消) - 過期清理服務設定
- `Expired` (已過期) - 由系統設定

### 5.3 Ticket.Status
- `Pending` (待付款) - 建立票券時自動設定
- `Unused` (未使用) - 付款成功後設定
- `Used` (已使用) - 驗票後設定
- `Expired` (已過期) - 過期清理服務設定

---

## 6. 修正優先順序

### 🔴 **高優先 (High Priority)**
1. **Theater Type**: 影響訂票流程,會導致 500 錯誤
2. **Seat Type**: 影響座位配置正確性

### 🟡 **中優先 (Medium Priority)**
3. **Movie Rating**: 影響內容分級顯示
4. **Movie Genre**: 影響電影分類和搜尋

---

## 7. 資料庫清理檢查

建議執行以下 SQL 檢查是否已存在非法值:

```sql
-- 檢查影廳類型
SELECT Id, Name, Type FROM Theater 
WHERE Type NOT IN ('Digital', 'IMAX', '4DX');

-- 檢查電影分級
SELECT Id, Title, Rating FROM Movie 
WHERE Rating NOT IN ('G', 'P', 'PG', 'R');

-- 檢查座位類型
SELECT Id, TheaterId, SeatType FROM Seat 
WHERE SeatType NOT IN ('Standard', 'Wheelchair', 'Aisle', 'Empty');
```

如果發現非法值,需要執行清理:

```sql
-- 修正中文影廳類型
UPDATE Theater 
SET Type = CASE 
    WHEN Type = '一般數位' THEN 'Digital'
    WHEN Type = 'IMAX' THEN 'IMAX'
    WHEN Type = '4DX' THEN '4DX'
    ELSE Type
END
WHERE Type NOT IN ('Digital', 'IMAX', '4DX');
```

---

## 8. 建議實作方式

### 方式 A: Service 層驗證 (推薦)
在 Service 層的 `CreateXxxAsync` 和 `UpdateXxxAsync` 方法中加入驗證邏輯。

**優點**:
- 業務邏輯集中管理
- 可提供詳細錯誤訊息
- 易於測試

### 方式 B: 使用 FluentValidation (進階)
建立 Validator 類別統一管理驗證規則。

```csharp
public class CreateTheaterRequestValidator : AbstractValidator<CreateTheaterRequestDto>
{
    public CreateTheaterRequestValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => new[] { "Digital", "IMAX", "4DX" }.Contains(t))
            .WithMessage("影廳類型無效。允許的值: Digital, IMAX, 4DX");
    }
}
```

---

## 總結

**共發現 4 個主要缺失**:
1. ❌ Theater Type - 無驗證 (高風險)
2. ❌ Seat Type - 部分驗證 (中風險)
3. ❌ Movie Rating - 無驗證 (中風險)
4. ❌ Movie Genre - 無驗證 (中風險)

建議優先修復 **Theater Type** 驗證,以避免訂票流程失敗。
