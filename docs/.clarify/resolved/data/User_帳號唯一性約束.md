# 釐清問題

User 的 account（帳號）是否需要唯一性約束？

# 定位

ERM：User 實體的 account 屬性唯一性規則

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 需要，account 必須唯一（DB Unique Constraint） |
| B | 不需要，同一帳號可被多人使用 |
| C | 部分唯一，同一影城內 account 唯一，但不同影城可重複 |
| D | 其他（請說明） |

# 影響範圍

- User 實體的唯一性約束設計
- 登入驗證邏輯
- 資料庫完整性

# 優先級

High
- 影響帳號系統的設計

---

# 【決議】✓

**選擇：B - 不需要 account 欄位，直接用 email 作為登入帳號**

## 依據

- 註冊流程只需要：名稱、信箱、密碼三個欄位
- 登入使用 email + password 驗證
- 無需額外的 account 欄位
- email 已設置 unique 約束

## 規格更新

- User 實體：移除 account 欄位（若存在）
- 確認 User.email 保持 unique 約束
- 登入驗證邏輯：使用 email + password
