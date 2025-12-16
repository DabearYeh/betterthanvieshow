# 釐清問題

MovieShowTime 的 show_date 與 show_time 屬性的資料格式為何？

# 定位

ERM：MovieShowTime 實體的 show_date 與 show_time 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | show_date: YYYY-MM-DD, show_time: HH:MM |
| B | show_date: ISO 8601 日期, show_time: HH:MM:SS |
| C | 合併為單一 datetime 欄位 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- MovieShowTime 實體的屬性定義
- 設定場次功能的輸入格式驗證
- 瀏覽場次功能的資料呈現
- 資料庫欄位型別選擇

# 優先級

Medium
- 影響資料格式驗證與儲存方式
- 影響前後台功能的資料處理

---

# 解決記錄

**使用者選擇:C (但保持為兩個欄位)**

**規格文件變更:**
- `spec/erm.dbml` - 更新 MovieShowTime 實體的屬性說明:
  - `show_date`: 格式 YYYY-MM-DD
  - `show_time`: 格式 HH:MM
  - `end_time`: 格式 HH:MM

**說明**: 保持 show_date 和 show_time 分離以便於查詢與篩選,但明確定義格式規範
