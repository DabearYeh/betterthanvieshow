# 釐清問題

MovieShowTime 的 show_date 與 show_time 屬性應該使用什麼資料格式？

# 定位

ERM：MovieShowTime 實體 show_date 與 show_time 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 合併為一個 datetime 屬性，格式：YYYY-MM-DD HH:MM |
| B | 分離為 show_date (YYYY-MM-DD) 和 show_time (HH:MM)，分別儲存 |
| C | 使用 Unix timestamp，儲存為數值 |

# 影響範圍

- MovieShowTime.show_date 與 show_time 的資料型別定義
- 場次排程的查詢與比較邏輯
- 時間衝突檢查的實現方式

# 優先級

Medium - 影響資料格式與查詢效率

---

# 解決記錄

- **回答**：A - 合併為一個日期時間屬性，格式：YYYY-MM-DD HH:MM
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：MovieShowTime 實體的 show_date 和 show_time 合併為 show_datetime (YYYY-MM-DD HH:MM)，更新跨屬性不變條件
