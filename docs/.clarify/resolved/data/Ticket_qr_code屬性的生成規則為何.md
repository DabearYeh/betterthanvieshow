# 釐清問題

Ticket 的 qr_code 屬性的生成規則為何？

# 定位

ERM：Ticket 實體的 qr_code 屬性

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 由 ticket_number 或 ticket_id 編碼生成 |
| B | 包含票券完整資訊的編碼 |
| C | 由外部服務生成 |
| Short | 提供其他簡短答案（<=5 字）|

# 影響範圍

- Ticket 實體的屬性定義
- 訂票功能的後置條件
- 驗票功能的驗證邏輯
- QR Code 掃描與解析邏輯

# 優先級

Medium
- 影響 QR Code 的生成與驗證邏輯
- 影響驗票功能的實作

---

# 解決記錄

**使用者選擇:A**

**規格文件變更:**
- `spec/erm.dbml` - 更新 Ticket 實體的 qr_code 屬性說明:
  - 由 ticket_number 編碼生成
  - 用於驗票功能
