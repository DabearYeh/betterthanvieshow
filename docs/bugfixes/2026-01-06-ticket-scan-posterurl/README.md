# 票券掃描 API 新增電影海報 URL

## 📅 修改日期
2026-01-06

## 🎯 修改目的
為 `GET /api/admin/tickets/scan` API 新增 `posterUrl` 欄位，讓前端能夠顯示電影海報。

## ❓ 問題描述
原本的票券掃描 API 回應中缺少電影海報（PosterUrl）欄位，導致前端在驗票畫面無法顯示電影海報，影響用戶體驗。

## 💡 解決方案

### 修改的檔案

#### 1. `Models/DTOs/TicketScanResponseDto.cs`
新增 `PosterUrl` 屬性：

```csharp
/// <summary>
/// 電影海報 URL
/// </summary>
public string PosterUrl { get; set; } = string.Empty;
```

#### 2. `Services/Implementations/TicketService.cs`
在 `ScanTicketByQrCodeAsync` 方法中設定 `PosterUrl`：

```csharp
var response = new TicketScanResponseDto
{
    TicketId = ticket.Id,
    TicketNumber = ticket.TicketNumber,
    Status = ticket.Status,
    MovieTitle = ticket.ShowTime.Movie.Title,
    PosterUrl = ticket.ShowTime.Movie.PosterUrl,  // ← 新增這一行
    ShowDate = ticket.ShowTime.ShowDate.ToString("yyyy-MM-dd"),
    ShowTime = ticket.ShowTime.StartTime.ToString(@"hh\:mm"),
    SeatRow = ticket.Seat.RowName,
    SeatColumn = ticket.Seat.ColumnNumber,
    SeatLabel = $"{ticket.Seat.RowName} 排 {ticket.Seat.ColumnNumber} 號",
    TheaterName = ticket.ShowTime.Theater.Name,
    TheaterType = ticket.ShowTime.Theater.Type
};
```

#### 3. `Controllers/TicketsController.cs`
更新 API 文件範例：

```csharp
/// **回應範例**：
/// ```json
/// {
///   "success": true,
///   "message": "成功取得票券資訊",
///   "data": {
///     "ticketId": 1,
///     "ticketNumber": "TKT-12345678",
///     "status": "Unused",
///     "movieTitle": "蝙蝠俠：黑暗騎士",
///     "posterUrl": "https://example.com/posters/dark-knight.jpg",  // ← 新增
///     "showDate": "2025-12-31",
///     "showTime": "14:30",
///     "seatRow": "D",
///     "seatColumn": 12,
///     "seatLabel": "D 排 12 號",
///     "theaterName": "2A",
///     "theaterType": "Digital"
///   }
/// }
/// ```
```

## 📊 API 回應變更對比

### 修改前
```json
{
  "success": true,
  "message": "成功取得票券資訊",
  "data": {
    "ticketId": 93,
    "ticketNumber": "49322368",
    "status": "Unused",
    "movieTitle": "奇怪的知識增加了",
    "showDate": "2026-01-11",
    "showTime": "14:00",
    "seatRow": "D",
    "seatColumn": 2,
    "seatLabel": "D 排 2 號",
    "theaterName": "大熊text廳",
    "theaterType": "IMAX"
  }
}
```

### 修改後 ✨
```json
{
  "success": true,
  "message": "成功取得票券資訊",
  "data": {
    "ticketId": 93,
    "ticketNumber": "49322368",
    "status": "Unused",
    "movieTitle": "奇怪的知識增加了",
    "posterUrl": "https://res.cloudinary.com/dojpfbtw8/image/upload/v1767589399/qki0gpoapwsthzugg0lm.jpg", // ← 新增
    "showDate": "2026-01-11",
    "showTime": "14:00",
    "seatRow": "D",
    "seatColumn": 2,
    "seatLabel": "D 排 2 號",
    "theaterName": "大熊text廳",
    "theaterType": "IMAX"
  }
}
```

## 🧪 測試結果

### 測試環境
- API Base URL: http://localhost:5041
- 測試日期: 2026-01-06
- 測試方法: PowerShell 腳本 + 手動驗證

### 測試案例

#### 測試案例 1: 票券 49322368 ✅
- **票券 ID**: 93
- **狀態**: Unused
- **電影**: 奇怪的知識增加了
- **posterUrl**: `https://res.cloudinary.com/dojpfbtw8/image/upload/v1767589399/qki0gpoapwsthzugg0lm.jpg`
- **結果**: ✅ 通過 - posterUrl 欄位存在且有效

#### 測試案例 2: 票券 50196649 ✅
- **票券 ID**: 94
- **狀態**: Unused
- **電影**: racecarporche555
- **posterUrl**: `https://res.cloudinary.com/dojpfbtw8/image/upload/v1767597667/f687pj7nserjrlhoza5f.png`
- **結果**: ✅ 通過 - posterUrl 欄位存在且有效

### 測試摘要
| 測試項目 | 結果 |
|---------|------|
| posterUrl 欄位存在 | ✅ 通過 |
| posterUrl 值有效 | ✅ 通過 |
| 原有欄位完整性 | ✅ 通過 |
| 向下相容性 | ✅ 通過 |

詳細測試結果請參考：[test_results.md](./test_results.md)

## 📝 資料來源

- **欄位**: `posterUrl`
- **資料來源**: `Movie.PosterUrl`
- **取得路徑**: `ticket.ShowTime.Movie.PosterUrl`
- **資料庫表**: `Movie` 表的 `PosterUrl` 欄位
- **圖片服務**: Cloudinary CDN

## ✅ 驗證清單

- [x] DTO 新增 PosterUrl 欄位
- [x] Service 層設定 PosterUrl 值
- [x] Controller 文件更新
- [x] 建置成功
- [x] 測試通過（2/2 測試案例）
- [x] 文件建立完成

## 🔄 影響範圍

### 向下相容性
✅ **完全向下相容** - 只是新增欄位，不影響現有欄位

### 前端影響
- ✅ 前端可選用此欄位
- ✅ 不使用該欄位的前端不受影響
- ✅ 無需強制更新

### 後端影響
- ✅ 無破壞性變更
- ✅ 無需資料庫遷移
- ✅ 無需版本升級

## 🎓 使用建議

### 給前端開發人員
1. 使用 `response.data.posterUrl` 取得電影海報 URL
2. 建議加入圖片載入失敗的預設圖片處理
3. posterUrl 可能為空字串，需要處理空值情況

**範例程式碼（React）：**
```jsx
<img 
  src={ticketData.posterUrl || '/default-poster.jpg'} 
  alt={ticketData.movieTitle}
  onError={(e) => e.target.src = '/default-poster.jpg'}
/>
```

**範例程式碼（Vue）：**
```vue
<img 
  :src="ticketData.posterUrl || '/default-poster.jpg'" 
  :alt="ticketData.movieTitle"
  @error="handleImageError"
/>
```

### 給後端開發人員
1. ✅ 修改已完成且測試通過
2. 未來可考慮加入縮圖版本（thumbnailUrl）
3. 建議在 Movie 資料驗證中確保 PosterUrl 格式正確

## 📚 相關文件

- [快速參考](./QUICK_REFERENCE.md) - 快速查看修改重點
- [測試腳本](./test_scan_posterurl.ps1) - PowerShell 測試腳本
- [測試結果](./test_results.md) - 詳細測試結果報告

## 🔗 相關 API

- `GET /api/admin/tickets/scan` - 掃描票券 QR Code
- `POST /api/admin/tickets/{ticketId}/validate` - 執行驗票

## 📌 備註

- 此修改為功能增強，非 bug 修復
- posterUrl 的圖片來源為 Cloudinary CDN
- 支援 .jpg 和 .png 格式
- URL 結構：`/image/upload/v{version}/{public_id}.{format}`

---

**修改人員**: Antigravity AI  
**修改日期**: 2026-01-06  
**測試狀態**: ✅ 通過
