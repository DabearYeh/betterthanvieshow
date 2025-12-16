# 釐清問題

票券的原價（original_price）和折扣後價格（final_price）如何計算？是否存在折扣規則或優惠邏輯？

# 定位

ERM：Ticket.original_price, Ticket.final_price
Feature：訂票 → 隱含的票價邏輯

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 無折扣：original_price = final_price，兩者由場次設定時確定 |
| B | 簡單折扣：根據座位類型（如 VIP 加價）計算原價，支持固定折扣 |
| C | 複雜折扣：支持多種折扣規則（會員、時間段、組合購買等）組合計算 |
| D | 動態定價：根據座位熱度、時間等動態調整價格 |
| Short | 其他定價邏輯 |

# 影響範圍

- Ticket 實體的票價屬性設計
- 訂票流程的總金額計算
- Order.total_amount 的計算邏輯
- 支付系統的金額驗證

# 優先級

High

---

**說明**：規格中未定義票價的來源與計算方式。MovieShowTime 實體中也未見 price 屬性，需要明確票價管理的設計。

---
# 解決記錄

- **回答**：此問題與第 2 題「訂票_original_price和final_price的計算邏輯.md」（.clarify/data/ 版本）相同
- **更新的規格檔**：已在第 2 題中處理
- **變更內容**：
  - 選擇 A - 簡單模型（無折扣）
  - 不需要 original_price 和 final_price 欄位
  - Ticket.price 根據影廳類型決定（一般數位 300 元、4DX 380 元、IMAX 380 元）
  - Order.total_price = Σ(Ticket.price)
  - 詳細內容請參考第 2 題的解決記錄
  - 此檔案為重複項目，已在 .clarify/data/ 版本中完整處理
