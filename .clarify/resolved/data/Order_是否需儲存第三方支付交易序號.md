# 釐清問題

Order 實體是否需要儲存來自第三方支付（如 LINE Pay）的回傳交易序號（Transaction ID）？

# 定位

ERM：Order 實體
屬性：目前無相關欄位

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 是，新增 `transaction_id` 欄位以利對帳與退款查詢 |
| B | 否，僅需記錄內部訂單編號 (`order_number`) 即可 |
| Short | Format: Short answer (<=5 words) |

# 影響範圍

- Entity: Order
- Feature: 訂票 (支付成功回調時需儲存 ID)

# 優先級

Medium

---
# 解決記錄

- **回答**：A - 是，新增 `transaction_id` 欄位以利對帳與退款查詢
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：在 Order 實體中新增 `payment_transaction_id` 欄位
