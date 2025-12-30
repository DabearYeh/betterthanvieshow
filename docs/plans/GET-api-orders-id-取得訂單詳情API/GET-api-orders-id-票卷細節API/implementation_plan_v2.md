# 實作計畫：取得訂單詳情 API (GET /api/orders/{id}) - 修正 v2 (票卷細節)

## 1. 概述 (v2 更新)
本計畫為 `GET /api/orders/{id}` 的功能增強版。除了原有的訂單基礎資訊外，現在需要在回應中包含**詳細的票卷資訊**，以支援前端顯示票卷細節頁面（包含 QR Code 與票卷狀態）。

- **目標 API**: `GET /api/orders/{id}`
- **主要變更**: 擴充座位列表的回應結構，加入 QR Code 生成內容與票卷狀態。

## 2. 變更項目

### DTO 變更
將原有的 `SeatInfoDto` 替換為功能更強大的 `TicketDetailDto`。

#### [NEW] TicketDetailDto
```csharp
public class TicketDetailDto
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; }
    public int SeatId { get; set; }
    public string RowName { get; set; }
    public int ColumnNumber { get; set; }
    
    // [New] 票卷狀態 (Pending, Unused, Used, Expired)
    public string Status { get; set; }
    
    // [New] QR Code 前端生成內容 (JSON Format)
    public string QrCodeContent { get; set; }
}
```

#### [MODIFY] OrderDetailResponseDto
- 屬性 `Seats` 的型別由 `List<SeatInfoDto>` 更改為 `List<TicketDetailDto>`。

### Service 層邏輯更新 (OrderService)
在 `GetOrderDetailAsync` 方法中需增加邏輯：
1.  從資料庫的 `Ticket` 實體讀取狀態 (`Status`)。
2.  生成 `QrCodeContent` 字串，格式為 JSON：
    ```json
    {
      "ticketNumber": "...",
      "showTimeId": 7,
      "seatId": 8
    }
    ```

## 3. API 回應範例 (v2)

```json
{
  "success": true,
  "message": "成功取得訂單詳情",
  "data": {
    "orderId": 12,
    "orderNumber": "#LOG-12152",
    // ... 其他欄位不變 ...
    "seats": [
      {
        "ticketId": 54,
        "ticketNumber": "79609841",
        "seatId": 8,
        "rowName": "H",
        "columnNumber": 12,
        "status": "Pending",  // [New]
        "qrCodeContent": "{\"ticketNumber\":\"79609841\",\"showTimeId\":7,\"seatId\":8}" // [New]
      }
    ],
    // ...
  }
}
```

## 4. 驗證與測試
- 建立新的測試腳本 `test-order-detail-v2.http`。
- 驗證重點：
    - 回傳的 `seats` 陣列中是否包含 `qrCodeContent`。
    - 確認 `qrCodeContent` 的格式是否正確。
    - 確認無破壞性變更（既有前端若不使用 QR Code 應不受影響，但需注意 DTO 改名或欄位型別變更對強型別前端的影響）。

## 舊版計畫內容 (保留參考)
*(以下為原始 v1 計畫內容)*

### 1. 概述
實作訂單詳情查詢 API，用於「訂單確認與支付選擇」頁面... (略)
