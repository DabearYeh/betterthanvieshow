# 釐清問題

Ticket 的 QR Code 應該何時生成？

# 定位

ERM：Ticket 實體 qr_code 屬性，訂票流程

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 票券建立時立即生成 |
| B | 付款成功後生成 |
| C | 驗票時生成 |

# 影響範圍

- Ticket.qr_code 生成時機
- 訂票.feature 中的 QR Code 相關規則
- 驗票.feature 中的驗票邏輯

# 優先級

High - 影響票券的安全性與可用性

---

# 【決議】

**選擇：A - 票券建立時立即生成**

## 依據

- Ticket 建立於「確認訂票」時
- QR Code 由 ticket_number 編碼生成（確定性生成）
- 使用者確認訂票後立即可以看到 QR Code
- 驗票時直接掃描 QR Code

## 規格更新

- Ticket 實體：qr_code 在建立時生成（來自 ticket_number 的編碼）
- 訂票.feature：確認訂票時產生 Order + Ticket + QR Code
- 驗票.feature：驗票時使用現有的 QR Code
