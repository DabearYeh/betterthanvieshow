# 釐清問題

座位配置中，如何區分「座位」、「走道」、「殘障座」等類型？

# 定位

ERM：Seat 實體的座位類型定義、is_active 屬性的用途

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 使用 seat_type 欄位區分（值：regular、vip、disabled、aisle）；is_active 用於標記座位是否可用 |
| B | 「走道」不建立 Seat 記錄，只有實際座位才建立 Seat |
| C | 使用 is_seat 欄位區分是否為座位，seat_category 區分座位類型 |
| D | 其他（請說明） |

# 影響範圍

- Seat 實體的屬性設計
- total_seats 的計算方式（是否包含走道？）
- 座位配置繪製的邏輯
- 訂票時座位選擇的規則

# 優先級

High
- 影響座位配置的資料結構
- 影響 total_seats 的定義

---

---

# 解決記錄

- **回答**：A - 使用 seat_type 欄位區分（regular、vip、disabled、aisle）；is_active 用於標記座位是否可用
- **更新的規格檔**：spec/erm.dbml
- **變更內容**：Seat 實體 seat_type 屬性已定義支援多種類型（一般座、VIP、雙人座等），seat_type 可用於區分包括走道在內的所有類型；is_active 用於標記座位可用性

