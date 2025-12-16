# 訂票_Order的total_amount如何從Ticket計算

Order.total_amount 應如何從該訂單的 Ticket 之 final_price 計算？是否支援整單折扣？

# 定義

ERM：Order.total_amount, Ticket.final_price 之間的關係

# 多選項

| 選項 | 描述 |
|--------|-------------|
| A | 簡單求和：total_amount = SUM(Ticket.final_price) |
| B | 支援訂單折扣：先求和，再套用訂單折扣 |
| C | 支援多層折扣：先套用票券折扣，再套用訂單折扣 |
| Short | 其他計算邏輯 |

# 影響範圍

- Order 與 Ticket 之間的價格關係
- 訂票結算流程的設計
- 支付驗證與審計邏輯
- 退款計算的精確性

# 優先級

High

---

**說明**：訂單總金額的計算邏輯未明確,需要確認是否支援整單折扣及具體計算方式。

---

# 解決記錄

- **決議**：A  簡單求和，total_amount = Σ(ticket.final_price)
- **理由**：無折扣、無優惠券，直接求和
- **更新規格檔案**：spec/features/訂票.feature、spec/erm.dbml Order.total_amount
