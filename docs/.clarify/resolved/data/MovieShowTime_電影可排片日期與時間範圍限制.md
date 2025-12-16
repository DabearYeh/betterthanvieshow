# 釐清問題

MovieShowTime 可以設定的放映日期與時間是否有限制？例如：是否只能排期在 release_date 之後？是否有場次持續時間計算（防止時間衝突）？

# 定位

ERM：MovieShowTime 實體的 show_date、start_time 屬性，以及與 Movie.release_date 的關係

Feature：設定場次.feature

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | show_date 必須 >= Movie.release_date，且必須在電影發行日期範圍內 |
| B | show_date 沒有限制，可任意設定 |
| C | 場次排期考慮電影片長，自動計算結束時間防止衝突 |
| D | 場次排期只需確保同影廳同時間不重疊，不考慮電影時長 |
| Short | 其他規則 |

# 影響範圍

- 設定場次功能的驗證邏輯
- 場次時間衝突檢測的實作方式
- 電影排片的業務規則

# 優先級

High
- High：影響場次設定功能的核心邏輯與驗證規則

---
# 解決記錄

- **回答**：A+C - show_date 必須在 [release_date, end_date] 範圍內，且考慮電影片長防止衝突
- **更新的規格檔**：無需修改（當前設計已符合需求）
- **變更內容**：
  - 決策：場次排片日期必須在電影上映期間內
  - 驗證規則：show_date >= Movie.release_date AND show_date <= Movie.end_date
  - 時間衝突檢查：同一影廳同一日期的場次，[start_time, start_time+duration) 不可重疊
  - 重映策略：若電影下映後需重映，建立新電影記錄
  - 當前 erm.dbml 中 MovieShowTime 的定義已完整描述此邏輯
