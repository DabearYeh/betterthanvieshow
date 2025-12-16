# 釐清問題

場次的結束時間（end_time）是否應該根據電影時長自動計算？或者允許手動設定？兩者之間是否有驗證邏輯？

# 定位

ERM：MovieShowTime.show_datetime, MovieShowTime.end_time, Movie.duration 之間的關係

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 自動計算：end_time = show_time + Movie.duration（分鐘轉換為時間） |
| B | 手動設定：end_time 完全由管理者指定，系統不進行自動計算 |
| C | 混合模式：系統先計算建議值，但允許管理者調整（如加入中場休息時間） |
| D | 檢查邏輯：end_time 必須 > show_time，且至少 = show_time + Movie.duration |
| Short | 其他邏輯 |

# 影響範圍

- MovieShowTime 實體的屬性關係
- 設定場次功能的實現邏輯
- 場次衝突檢查的計算邏輯（時間重疊判斷）
- 驗票功能的過期判斷邏輯

# 優先級

High

---

**說明**：目前規格未明確說明場次結束時間的計算或設定方式，這對於時間衝突檢查與驗票過期邏輯都很重要。

---
# 解決記錄

- **回答**：A - 自動計算 end_time = start_time + Movie.duration
- **更新的規格檔**：無需修改（當前設計已符合需求）
- **變更內容**：
  - 決策：場次結束時間動態計算，不儲存在資料庫中
  - 計算邏輯：end_time = start_time + Movie.duration（分鐘）
  - 應用層計算，避免數據冗餘
  - 用於場次衝突檢查和驗票過期判斷
  - 當前 erm.dbml 中 MovieShowTime 的 Note 已明確說明此邏輯
