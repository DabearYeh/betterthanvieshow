# 釐清問題

Ticket 的 qr_code 屬性應該如何生成？

# 定位

ERM：Ticket 實體 qr_code 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 由 ticket_number 編碼生成 |
| B | 全局唯一的隨機 UUID |
| C | 包含訂單、座位、場次等多個資訊的複合編碼 |

# 影響範圍

- Ticket.qr_code 的生成演算法
- QR Code 的內容結構與驗證邏輯
- 驗票系統的掃描與解析方式

# 優先級

Medium - 影響驗票效率與安全性

---

# 解決記錄

- **回答**：A - 由票券編號 (ticket_number) 編碼生成
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Ticket 實體 qr_code 屬性已確認生成規則（由 ticket_number 編碼生成），無需修改
