# 釐清問題

前台使用者瀏覽場次時，是否只顯示 OnSale 狀態的日期場次？

# 定位

ERM：DailySchedule.status 與前台可見性
Feature：瀏覽場次.feature

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 僅 OnSale：前台只顯示 OnSale 狀態的場次，Draft 狀態對使用者不可見 |
| B | 全部顯示：Draft 和 OnSale 場次都顯示，但 Draft 場次標記為「即將開賣」 |
| C | Draft 可預覽：Draft 場次顯示但不可訂票，僅供預覽 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- 瀏覽場次功能的查詢條件
- 前台 UI 顯示邏輯
- 訂票功能的前置條件驗證

# 優先級

High
- 直接影響前台使用者的場次可見性與訂票流程

---
# 解決記錄

- **回答**：A - 僅 OnSale：前台只顯示 OnSale 狀態的場次，Draft 狀態對使用者不可見
- **更新的規格檔**：spec/features/瀏覽場次.feature
- **變更內容**：新增規則「只顯示販售中狀態的場次」，包含 OnSale 可見與 Draft 不可見的 Example
