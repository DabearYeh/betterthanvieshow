# 釐清問題

Theater 的 total_seats 屬性是否需要與實際 Seat 表中該影廳的座位數量保持一致？

# 定位

ERM：Theater 實體的 total_seats 屬性，以及 Theater 與 Seat 之間的關係

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | total_seats 必須等於實際 Seat 數量（強制一致性） |
| B | total_seats 僅為顯示用途，不需與實際 Seat 數量一致 |
| C | total_seats 為座位容量上限，實際 Seat 數量不得超過此值 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- Theater 實體的跨屬性不變條件
- 設定座位配置功能的後置條件
- 建立影廳功能的驗證規則
- 編輯影廳功能的驗證規則

# 優先級

High
- 影響 Theater 與 Seat 的一致性約束定義
- 影響座位配置相關功能的驗證邏輯

---

# 解決記錄

**使用者選擇:A**

**規格文件變更:**
- `spec/erm.dbml` - 更新 Theater 實體的跨屬性不變條件:
  - 新增約束:total_seats 必須等於該影廳實際的 Seat 數量(強制一致性)

---

# 解決記錄（更新）

- **回答**：A - total_seats 必須等於實際 Seat 數量（強制一致性）
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Theater 實體 total_seats 屬性必須與該影廳實際 Seat 數量保持一致；修改座位配置時必須同時更新 total_seats

