# 釐清問題

Ticket 的 ticket_number 屬性應該如何生成？

# 定位

ERM：Ticket 實體 ticket_number 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 全局自增序號，格式：00000001、00000002... |
| B | 日期 + 序號，格式：YYYYMMDD-XXX (日期+同日序號) |
| C | 隨機字符串 |

# 影響範圍

- Ticket.ticket_number 的生成演算法
- 票券編號的唯一性保證
- 驗票時的票券查找邏輯

# 優先級

Medium - 影響票券識別與驗票效率

---

# 解決記錄

- **回答**：B - 日期 + 序號，格式：YYYYMMDD-XXX (同日期內的序號)
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Ticket 實體 ticket_number 生成規則已確認（格式 YYYYMMDD-XXX），無需修改
