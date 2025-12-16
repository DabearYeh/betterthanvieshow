# 釐清問題

Order 訂單是否需要關聯到 User（使用者）？

# 定位

ERM：Order 實體與 User 實體的關聯關係

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 需要，Order 應新增 user_id 外鍵，建立 User 1:N Order 關聯 |
| B | 不需要，Order 只記錄訂購人名稱/電話/email，無需關聯 User |
| C | 需要，但同時保留 customer_name、customer_phone、customer_email（便於查詢） |
| D | 其他（請說明） |

# 影響範圍

- Order 實體是否新增 user_id 外鍵
- 訂票功能的前置條件（是否需要使用者登入）
- 查詢訂票記錄功能的實現（按 user_id vs 按 customer_name）

# 優先級

High
- 影響訂票流程的設計
- 影響使用者登入的必要性

---

---

# 解決記錄

- **回答**：A - Order 應新增 user_id 外鍵，建立 User 1:N Order 關聯
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Order 實體新增 user_id 外鍵指向 User.id，建立 User 1:N Order 關聯

