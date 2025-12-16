# 釐清問題

Order 實體是否需要記錄時間戳記（created_at, paid_at, updated_at）？

# 定位

ERM：Order 實體相關時間屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 需要 created_at 和 updated_at，不需要 paid_at |
| B | 需要 created_at、paid_at、updated_at 三個時間戳記 |
| C | 不需要時間戳記，系統內部維護 |

# 影響範圍

- Order 實體屬性定義
- 查詢訂票記錄.feature 中的時間篩選
- 訂票流程的追蹤與審計

# 優先級

High - 影響訂單追蹤與業務分析

---

# 解決記錄

- **回答**：B - 需要訂單建立時間、付款完成時間、訂單更新時間三個時間戳記
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Order 實體時間戳記屬性已確認（created_at、paid_at、updated_at），無需修改
