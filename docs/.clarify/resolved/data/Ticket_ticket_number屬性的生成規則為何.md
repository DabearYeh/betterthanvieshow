# 釐清問題

Ticket 的 ticket_number 屬性的生成規則為何？

# 定位

ERM：Ticket 實體的 ticket_number 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 系統自動生成的唯一序號（如：UUID） |
| B | 有特定格式的編號（如：日期+序號） |
| C | 由外部系統生成 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- Ticket 實體的屬性定義
- 訂票功能的後置條件
- ticket_number 的唯一性約束
- 驗票功能的驗證邏輯

# 優先級

Medium
- 影響票券編號的生成與驗證邏輯
- 影響資料唯一性設計

---

# 解決記錄

**使用者選擇:B**

**規格文件變更:**
- `spec/erm.dbml` - 更新 Ticket 實體的 ticket_number 屬性說明:
  - 格式: YYYYMMDD-XXX (日期+序號)
  - 必須唯一
