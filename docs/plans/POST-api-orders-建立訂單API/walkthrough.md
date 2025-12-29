# 實作指南：建立訂單 API

本文件記錄 `POST /api/orders` 的實作步驟與注意事項，特別包含最後階段針對資料庫 Check Constraint 錯誤的修正。

## 1. DTO 定義
建立相關 Data Transfer Objects 以定義 API 介面。
- `CreateOrderRequestDto`: 接收 `showTimeId` 和 `seatIds`。
- `CreateOrderResponseDto`: 回傳訂單摘要與票券資訊。
- `SeatInfoDto`: 票券內的座位詳細資訊。

## 2. Repository Layer 擴充
- **IOrderRepository**: 新增 `CreateAsync`, `OrderNumberExistsAsync`.
- **ITicketRepository**: 新增 `CreateBatchAsync` (批次寫入), `TicketNumberExistsAsync`, `IsSeatOccupiedAsync`.
- **IShowtimeRepository**: 確保 `GetByIdWithDetailsAsync` 包含 `Theater` 資訊以供票價計算。

## 3. Service Layer 實作 (`OrderService`)
實作 `CreateOrderAsync` 方法，包含完整的業務邏輯getTransaction。
- **關鍵邏輯**:
  - 驗證 `DailySchedule` 是否為 `OnSale`。
  - 檢查每個座位是否已被佔用 (Concurrent conflict prevention)。
  - 動態計價策略 (IMAX, 4DX, 一般)。
  - 產生易讀的訂單編號。

## 4. Controller Layer 實作 (`OrdersController`)
- 新增 `HttpPost` Endpoint。
- 使用 `[Authorize]` 確保安全性。
- 從 `User.Claims` 解析 `UserId`。
- 統一錯誤處理：使用 `try-catch` 捕捉 `InvalidOperationException` 並轉換成適當的 HTTP 狀態碼 (400, 404, 409)。

## 5. 資料庫 Check Constraint 修正 (Troubleshooting)
在測試階段遇到 `500 Internal Server Error`，經診斷為 `SqlException`：
> The INSERT statement conflicted with the CHECK constraint "CHK_Order_Status".

**原因**：
原有設計使用中文狀態碼 (`未付款`, `待支付`)，但 SQL Server 的資料庫編碼或 Check Constraint 定義導致字串比對失敗。

**解決方案**：
全面改用英文狀態碼，並更新 DB Schema。

1. **Modify Entity Models**:
   - `Order.cs`: Status default -> `"Pending"`
   - `Ticket.cs`: Status default -> `"Pending"`

2. **Update DbContext**:
   - `CHK_Order_Status` -> `IN ('Pending', 'Paid', 'Cancelled')`
   - `CHK_Ticket_Status` -> `IN ('Pending', 'Unused', 'Used', 'Expired')`

3. **Update Service**:
   - `OrderService.cs`: 設定狀態為 `"Pending"`.

4. **Migration**:
   - 執行 `dotnet ef migrations add UpdateOrderStatusToEnglish`
   - 執行 `dotnet ef database update`

5. **Update Repositories**:
   - `TicketRepository.cs`: 更新 `IsSeatOccupiedAsync` 等方法的狀態查詢條件（由中文改為英文）。

## 6. 異常情境驗證 (Bug Fix)
在進行全面測試時，發現座位衝突檢測失效（明明座位已被訂購卻仍回傳 201）。

**診斷**：
雖然資料庫與 Entity 已更新為英文狀態，但 `TicketRepository.cs` 內的 LINQ 查詢條件仍在使用中文（如 `"待支付"`, `"未使用"`），導致 `IsSeatOccupiedAsync` 永遠回傳 `false`。

**解決方案**：
將 `TicketRepository.cs` 內所有涉及 `Status` 比對的字串同步更新為英文。

## 7. 最終驗證結果
使用 `scripts/test_orders_full_scenarios.ps1` 進行全情境測試：
- [x] **座位重複訂位 (409)**: 成功偵測並攔截。
- [x] **場次不存在 (404)**: 正確處理。
- [x] **座位不存在 (404)**: 正確處理。
- [x] **訂票數量限制 (400)**: 正確攔截（上限 6 張）。
- [x] **正常訂位流程 (201)**: 流程順暢且資料正確。

---

## 8. SignalR 與背景任務驗證

### SignalR 即時廣播
**實作狀態**：✅ 已整合

**程式碼變更**：
- [OrderService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/OrderService.cs)：注入 `IHubContext<ShowtimeHub>`，在訂單建立成功後廣播 `SeatStatusChanged` 事件。
- 事件格式：`{ showtimeId, seatIds, status: "sold" }`

**預期行為**：
- 訂單建立成功時：廣播 `status: "sold"`
- 訂單過期被取消時：廣播 `status: "available"`

### 背景自動清理服務
**測試方式**：啟動應用程式，觀察日誌輸出。

**結果**：✅ 成功
```
info: ExpiredOrderCleanupService 已啟動
info: 找到 10 筆過期訂單，開始清理
info: 訂單 #SGV-74120 已自動取消
info: 訂單 #KLG-74641 已自動取消
info: 訂單 #LOG-12152 已自動取消
info: 訂單 #LGU-74038 已自動取消
info: 訂單 #DZZ-82360 已自動取消
info: 訂單 #DFR-91197 已自動取消
info: 訂單 #VEX-10457 已自動取消
info: 訂單 #RTC-50711 已自動取消
info: 訂單 #NNR-96652 已自動取消
info: 訂單 #RCI-65534 已自動取消
```

**結論**：
- 背景服務在應用程式啟動後立即開始運作
- 成功偵測並清理了資料庫中 10 筆已過期的 Pending 訂單
- 每筆訂單的狀態已更新為 `Cancelled`
- 關聯票券狀態已更新為 `Expired`
- 座位資源已自動釋放，可供其他使用者訂購

**程式碼變更**：
- [ExpiredOrderCleanupService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Background/ExpiredOrderCleanupService.cs) (新增)：每分鐘掃描並自動取消過期訂單。
- [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)：註冊 `ExpiredOrderCleanupService` 為 Hosted Service。

## 9. 後續建議

1. **前端整合**：建議前端在進入選位頁面時連接 SignalR Hub，並監聽 `SeatStatusChanged` 事件即時更新 UI。
2. **日誌優化**：可考慮將背景服務的 Debug 級別日誌移至 Trace，減少生產環境日誌量。
3. **效能監控**：長期運作後可監控背景任務的執行時間，必要時調整掃描頻率。

