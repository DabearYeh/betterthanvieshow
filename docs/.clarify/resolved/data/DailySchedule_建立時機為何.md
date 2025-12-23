# 釐清問題

當某日期沒有任何場次時，是否需要建立空的 DailySchedule 記錄？

# 定位

ERM：DailySchedule 實體的建立時機
Feature：設定場次.feature - Rule: 新增場次時自動建立或關聯每日時刻表

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 按需建立：只有在新增第一個場次時才建立 DailySchedule |
| B | 預先建立：管理者可以先建立空的 DailySchedule，之後再新增場次 |
| C | 日期選擇時建立：當管理者在時刻表 UI 選擇某日期時自動建立 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- 設定場次功能的實作邏輯
- 時刻表 UI 的行為設計
- DailySchedule 記錄的生命週期管理

# 優先級

Medium
- 影響場次建立流程，但不影響核心資料模型

---
# 解決記錄

- **回答**：A - 按需建立：只有在新增第一個場次時才建立 DailySchedule
- **更新的規格檔**：spec/features/設定場次.feature
- **變更內容**：補充 Example「日期已有時刻表時關聯現有記錄」，明確說明不重複建立 DailySchedule
