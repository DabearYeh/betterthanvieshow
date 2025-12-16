# 釐清問題

Ticket 初次建立時的狀態應為何？

# 定位

ERM：Ticket 實體 status 屬性，規格定義中的所有可能狀態值

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 待驗證（待使用者驗票） |
| B | 待付款（待支付） |
| C | 已失效（初始狀態無效） |

# 影響範圍

- Ticket.status 初始值設定
- 訂票.feature 中關於票券狀態流程的規則
- 驗票.feature 中的驗票前置條件

# 優先級

High - 影響票券生命週期與核心業務流程

---

# 【決議】✓

**選擇：B - 待付款**

## 依據

- Order + Ticket 在「確認訂票」時同時產生
- Ticket 初始狀態 = 待付款（pending payment）
- 付款成功後狀態更新為待驗證
- 座位鎖定機制：Ticket 存在即鎖定座位（與狀態無關）

## 規格參考

來源：spec/features/訂票.feature

```gherkin
Rule: 確認訂單後必須付款
  When 使用者按下確認訂單鈕
  Then 系統建立 Order 與 Ticket（狀態：待付款）

Rule: 付款成功後更新訂單與票券狀態
  When 系統收到支付成功回調
  Then Order 狀態更新為已付款
  And Ticket 狀態更新為待驗證
```

## 設計說明

- Ticket 生命週期：待付款 → 待驗證 → 已驗票（或已失效）
- 座位可用性判斷：檢查是否存在任何狀態的 Ticket（付款失敗情況下座位仍需鎖定）
- 一張 Order 可包含多張 Ticket（多座位訂票）
