# 釐清問題

同一影廳內的 Seat，其 row_name 與 seat_number 的組合是否必須唯一？

# 定位

ERM：Seat 實體的跨屬性不變條件，涉及 row_name 與 seat_number 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 必須唯一（同一影廳內不可有重複的排號與座位號組合） |
| B | 不需唯一（允許重複的排號與座位號組合） |
| C | 僅 seat_number 需在影廳內唯一，row_name 可重複 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- Seat 實體的唯一性約束
- 設定座位配置功能的驗證規則
- 資料庫 Unique Constraint 設計

# 優先級

High
- 影響座位唯一性的核心約束定義
- 影響資料庫設計與驗證邏輯

---

# 解決記錄

**使用者選擇:A**

**規格文件變更:**
- `spec/erm.dbml` - 更新 Seat 實體的跨屬性不變條件:
  - 新增約束:同一個 theater_id 內,(row_name, seat_number) 組合必須唯一(需 DB Unique Constraint)
