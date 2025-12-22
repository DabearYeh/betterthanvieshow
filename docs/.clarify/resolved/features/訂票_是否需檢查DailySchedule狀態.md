# 釐清問題

使用者訂票時，系統是否需要檢查該場次日期的 DailySchedule 狀態為 OnSale？

# 定位

Feature：訂票.feature

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 需要檢查：訂票前驗證 DailySchedule.status = OnSale，非 OnSale 禁止訂票 |
| B | 不需要：訂票只檢查場次存在與座位可用，不檢查 DailySchedule 狀態 |
| C | 前端過濾：前端只顯示 OnSale 場次，後端不再重複驗證 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- 訂票功能的前置條件規則
- 訂票 API 的驗證邏輯
- 場次與 DailySchedule 的查詢邏輯

# 優先級

High
- 直接影響訂票流程的正確性

---
# 解決記錄

- **回答**：A - 需要檢查：訂票前驗證 DailySchedule.status = OnSale，非 OnSale 禁止訂票
- **更新的規格檔**：spec/features/訂票.feature
- **變更內容**：新增規則「場次日期的時刻表必須為販售中狀態」，包含 OnSale 可訂與 Draft 禁止的 Example
