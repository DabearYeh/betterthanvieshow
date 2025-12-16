# 釐清問題

MovieShowTime 實體是否需要 end_time 屬性來記錄場次結束時間？

# 定位

ERM：MovieShowTime 實體，與 Ticket 過期判斷相關

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 需要，新增 end_time 欄位記錄場次結束時間 |
| B | 不需要，用 show_time + Movie.duration 計算 |
| C | 可選，僅當需要考慮中場休息時才新增 |

# 影響範圍

- MovieShowTime 實體屬性定義
- 驗票.feature 中判斷票券過期的規則
- 設定場次.feature 中的時間衝突檢查

# 優先級

High - 影響場次排程與驗票邏輯

---

# 解決記錄

- **回答**：A - 需要，新增 end_time 欄位記錄場次結束時間
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：MovieShowTime 實體已包含 end_time 屬性（格式：HH:MM，包含中場休息等時間），無需修改

