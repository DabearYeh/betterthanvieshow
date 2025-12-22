# 釐清問題

當 DailySchedule 的所有場次都被刪除後，該 DailySchedule 記錄是否需要刪除？

# 定位

ERM：DailySchedule 實體的生命週期與刪除規則
Feature：刪除場次.feature

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 保留：即使沒有場次，DailySchedule 記錄仍保留，狀態維持 Draft |
| B | 自動刪除：當該日期的最後一個場次被刪除時，自動刪除 DailySchedule |
| C | 手動刪除：提供管理者手動刪除空 DailySchedule 的功能 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- 刪除場次功能的後置條件
- DailySchedule 的 CRUD 設計
- 時刻表 UI 顯示邏輯

# 優先級

Medium
- 影響資料一致性管理

---
# 解決記錄

- **回答**：A - 保留：即使沒有場次，DailySchedule 記錄仍保留，狀態維持 Draft
- **更新的規格檔**：spec/features/刪除場次.feature
- **變更內容**：新增規則「刪除最後一個場次時保留時刻表記錄」，明確時刻表不會自動刪除
