# 釐清問題

訂票中 original_price 和 final_price 的計算邏輯是什麼？兩者有何區別？

# 定位

ERM：Ticket 和 Order 的價格欄位  
Feature：訂票 → 顯示票價  
Business Logic：折扣、優惠、稅費計算

# 背景信息

根據 spec.md：
- Ticket.price = 300 元（固定票價）
- Order.total_price = 訂單總金額

但需要釐清：
- original_price（原價）是什麼？
- final_price（最終價）是什麼？
- 兩者如何計算？
- 是否涉及折扣或優惠？

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 簡單模型：無折扣，original_price = final_price = ticket_count × 300 |
| B | 折扣模型：original_price = ticket_count × 300；final_price = original_price × discount_rate（如有折扣） |
| C | 進階模型：original_price = ticket_count × 300；final_price = original_price - promotion - coupon + tax |
| D | 自訂模型：其他具體計算邏輯 |

# 影響範圍

- Ticket 實體的價格欄位設計
- Order 實體的總金額計算
- 訂票頁面的價格顯示
- 支付系統的金額確認
- 後台訂單管理的數據一致性

# 優先級

High ⭐⭐

---

**說明**：
當前僅明確票價為固定 300 元，但訂票流程中可能涉及折扣、促銷、稅費等因素，需要確定計算邏輯。

# 相關 Feature 檔案

- `訂票.feature` — 尚未提及價格計算
- `查詢訂票記錄.feature` — 訂票記錄中顯示價格

# 相關欄位

- Ticket.price
- Order.total_price
- （可能需要新增：Ticket.original_price、Ticket.final_price、Order.total_original_price、Order.total_final_price）

---
# 解決記錄

- **回答**：此問題與第 2 題相同，已在第 2 題中處理
- **更新的規格檔**：已在第 2 題中處理
- **變更內容**：
  - 選擇 A - 簡單模型（無折扣）
  - 不需要 original_price 和 final_price 欄位
  - Ticket.price 根據影廳類型決定（一般數位 300 元、4DX 380 元、IMAX 380 元）
  - Order.total_price = Σ(Ticket.price)
  - 詳細內容請參考第 2 題的解決記錄和 spec/features/訂票.feature
  - 此檔案為重複項目
