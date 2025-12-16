# 釐清問題

Order 實體是否需要記錄訂購人資訊？

# 定位

ERM：Order 實體相關屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 需要記錄，新增 customer_name, customer_phone, customer_email 欄位 |
| B | 不需要記錄，訂購人資訊由系統內部維護 |
| C | 非必填，可選記錄訂購人資訊 |

# 影響範圍

- Order 實體屬性定義
- 訂票.feature 中關於訂購人資訊的規則
- 查詢訂票記錄.feature 的查詢條件

# 優先級

High - 影響核心資料模型與業務流程

---

# 解決記錄

- **回答**：B - 不需要記錄 customer_name 和 customer_email，改由 user_id 關聯 User 表取得；保留 customer_phone
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Order 實體移除 customer_name 和 customer_email，保留 customer_phone；新增 user_id 外鍵

