# 釐清問題

Movie.release_status 屬性應該有哪些可能的值？

# 定位

ERM：Movie 實體 release_status 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 0 = 未上映, 1 = 上映中, 2 = 已下檔 |
| B | 'coming' = 即將上映, 'running' = 上映中, 'ended' = 已結束 |
| C | 'draft' = 草稿, 'active' = 上映, 'archived' = 下檔 |

# 影響範圍

- Movie.release_status 值域定義
- 瀏覽電影.feature 中判斷上映狀態的規則
- 建立電影.feature 與編輯電影.feature 中的狀態管理

# 優先級

High - 影響電影上映狀態的判斷邏輯

---

# 解決記錄

- **回答**：B - 使用英文代碼：ccoming = 即將上映、running = 上映中、ended = 已結束
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Movie 實體 release_status 屬性的值域定義為：coming（未上映）、running（上映中）、ended（已下檔）
