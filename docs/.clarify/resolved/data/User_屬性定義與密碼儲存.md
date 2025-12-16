# 釐清問題

User 實體的必填屬性為何？密碼如何儲存？

# 定位

ERM：User 實體的屬性定義（name, account, password）

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | User 只需 id、account、password、name；密碼以明文存儲 |
| B | User 只需 id、account、password、name；密碼以加密方式存儲（如 bcrypt） |
| C | User 需 id、account、password、name、email、phone；密碼以加密方式存儲 |
| D | 其他（請說明） |

# 影響範圍

- User 實體的屬性設計
- 資料庫安全性
- 登入功能的實現

# 優先級

High
- 影響系統安全性
- 影響登入功能設計

---

---

# 解決記錄

- **回答**：User 只需 id、name、email、password；密碼以加密方式存儲（bcrypt）；email 唯一；無需 account、phone
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：新增 User 實體，包含 id、name、email (unique)、password 四個屬性

