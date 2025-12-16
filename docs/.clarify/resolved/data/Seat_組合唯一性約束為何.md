# 釐清問題

Seat 實體中，(row_name, seat_number) 組合是否必須在同一影廳內唯一？

# 定位

ERM：Seat 實體唯一性約束

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 必須唯一，同一影廳內不能有重複的 (row_name, seat_number) 組合 |
| B | 不需要，允許重複 |
| C | 可選，由系統配置決定 |

# 影響範圍

- Seat 實體的唯一性約束設計
- 座位識別與管理邏輯
- 資料庫 Unique Constraint 定義

# 優先級

High - 影響座位管理的資料完整性

---

# 解決記錄

- **回答**：A - 必須唯一，同一影廳內不能有重複的 (row_name, seat_number) 組合
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Seat 實體已定義跨屬性不變條件：同一影廳內 (row_name, seat_number) 組合必須唯一，無需修改

