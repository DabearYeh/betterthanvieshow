# 快速參考 - 票券掃描 API 新增 posterUrl

## 🎯 一句話總結
為票券掃描 API 新增 `posterUrl` 欄位，讓前端可以顯示電影海報。

## 📝 修改的檔案（3 個）

1. `Models/DTOs/TicketScanResponseDto.cs` - 新增屬性
2. `Services/Implementations/TicketService.cs` - 設定值
3. `Controllers/TicketsController.cs` - 更新文件

## 🔧 程式碼修改

### DTO（新增）
```csharp
public string PosterUrl { get; set; } = string.Empty;
```

### Service（新增）
```csharp
PosterUrl = ticket.ShowTime.Movie.PosterUrl,
```

### Controller（新增文件）
```csharp
///     "posterUrl": "https://example.com/posters/dark-knight.jpg",
```

## 📊 API 回應範例

### 請求
```http
GET /api/admin/tickets/scan?qrCode=49322368
Authorization: Bearer {token}
```

### 回應
```json
{
  "success": true,
  "message": "成功取得票券資訊",
  "data": {
    "ticketId": 93,
    "ticketNumber": "49322368",
    "movieTitle": "奇怪的知識增加了",
    "posterUrl": "https://res.cloudinary.com/dojpfbtw8/image/upload/v1767589399/qki0gpoapwsthzugg0lm.jpg",
    "showDate": "2026-01-11",
    "showTime": "14:00",
    "seatLabel": "D 排 2 號",
    "theaterName": "大熊text廳"
  }
}
```

## ✅ 測試狀態
- ✅ 建置成功
- ✅ 測試通過（2/2）
- ✅ 向下相容

## 🚀 前端使用

### React
```jsx
<img src={data.posterUrl || '/default.jpg'} alt={data.movieTitle} />
```

### Vue
```vue
<img :src="data.posterUrl || '/default.jpg'" :alt="data.movieTitle" />
```

### JavaScript
```js
const posterUrl = response.data.posterUrl || '/default-poster.jpg';
```

## 📌 重點提醒

1. **新增欄位**: `posterUrl`
2. **資料來源**: `Movie.PosterUrl`
3. **圖片服務**: Cloudinary CDN
4. **向下相容**: ✅ 是
5. **必填欄位**: ❌ 否（可能為空）
6. **需要遷移**: ❌ 否

## 🔗 相關連結

- [完整文件](./README.md)
- [測試結果](./test_results.md)
- [測試腳本](./test_scan_posterurl.ps1)
