# 釐清問題

DailySchedule 與 MovieShowTime 之間的關聯方式是透過隱式 FK (schedule_date = show_date) 還是顯式 FK (daily_schedule_id)?

# 定位

ERM：DailySchedule 實體與 MovieShowTime 實體之間的關聯定義

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 隱式關聯：MovieShowTime.show_date = DailySchedule.schedule_date，不需新增欄位 |
| B | 顯式關聯：在 MovieShowTime 新增 daily_schedule_id 外鍵欄位 |
| C | 混合模式：兩者都保留，隱式關聯用於查詢，顯式關聯用於 DB 限制 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- DailySchedule 實體定義
- MovieShowTime 實體定義
- 設定場次功能：新增場次時是否需同時更新 daily_schedule_id
- 開始販售時刻表功能：狀態檢查的查詢邏輯

# 優先級

High
- 此決策直接影響資料庫 schema 設計與場次 CRUD 的實作邏輯

---
# 解決記錄

- **回答**：A - 隱式關聯：透過 DailySchedule.schedule_date = MovieShowTime.show_date 比對，不需新增外鍵欄位
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：更新 DailySchedule 實體的 Note，明確說明隱式關聯方式
