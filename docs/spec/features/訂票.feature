Feature: 訂票
  前台使用者可以訂票

  Rule: 必須選擇電影和場次
    Example: 成功選擇電影和場次
      Given 使用者進入訂票頁面
      When 使用者選擇電影「復仇者聯盟」
      And 使用者選擇場次「2025-12-20 14:00」
      Then 系統導向座位選擇畫面

  Rule: 必須選擇座位
    Example: 成功選擇座位
      Given 座位配置已顯示
      When 使用者選擇座位 A3、A4
      Then 系統記錄已選座位

  Rule: 一次訂單最多可訂 6 張票
    Example: 訂購 6 張票成功
      Given 使用者選擇 6 個座位
      When 使用者進行確認
      Then 系統建立 Order 和 6 張 Ticket 記錄

    Example: 訂購超過 6 張票失敗
      Given 使用者嘗試選擇 7 個座位
      When 使用者嘗試確認
      Then 操作失敗

  Rule: 只能選擇未被訂走的座位
    Example: 座位已被訂走時無法選擇
      Given 座位 B3 已有有效票券
      When 使用者嘗試選擇座位 B3
      Then 系統禁止選擇

  Rule: 同一座位在同一場次只能被一人購買
    Example: 座位唯一性檢查
      Given 座位 C5 在場次「2025-12-20 14:00」已被訂購
      When 另一使用者嘗試訂購同座位
      Then 操作失敗

  Rule: 訂單編號格式為 #ABC-12345
    Example: 訂單編號自動生成
      Given 使用者確認訂單
      When 系統建立 Order 記錄
      Then Order.order_number 符合格式「#ABC-12345」

  Rule: 確認訂單後進入付款流程
    Example: 訂單建立後跳轉付款
      Given 使用者已選擇座位並確認
      When 系統建立 Order 和 Ticket 記錄
      Then 系統跳轉到 LINE Pay 支付頁面
      And 支付方式：LINE Pay（第三方支付）

  Rule: 支付方式為 LINE Pay
    Example: LINE Pay 支付完成
      Given 使用者跳轉至 LINE Pay 支付頁面
      When 使用者完成 LINE Pay 支付
      Then 系統記錄支付成功
      And Order.status 更新為「已付款」
      And Ticket.status 更新為「未使用」

  Rule: 未付款訂單 5 分鐘後自動取消
    Example: 超時自動取消
      Given Order 建立時間為「2025-12-20 14:00:00」
      When 至「2025-12-20 14:05:01」仍未完成付款
      Then 系統自動取消 Order
      And 釋放預留座位


  Rule: 票券包含完整資訊
    Example: 票券包含電影、日期、影廳、座位、時間、影廳類型
      Given Ticket 已建立
      When 使用者查看票券
      Then 票券 QR Code 包含：
        | field | value |
        | 電影名 | 復仇者聯盟 |
        | 日期 | 2025-12-20 |
        | 影廳 | IMAX 廳 |
        | 座位 | A3 |
        | 時間 | 14:00 |
        | 影廳類型 | IMAX |

  Rule: 票券狀態為未使用
    Example: 新票券狀態為未使用
      Given Ticket 剛建立且付款完成
      When 系統生成票券
      Then Ticket.status 為「未使用」

  Rule: 票價根據影廳類型決定
    Example: 一般數位廳票價為 300 元
      Given 使用者選擇一般數位廳的場次
      When 系統計算票價
      Then Ticket.price 為 300 元

    Example: 4DX 廳票價為 380 元
      Given 使用者選擇 4DX 廳的場次
      When 系統計算票價
      Then Ticket.price 為 380 元

    Example: IMAX 廳票價為 380 元
      Given 使用者選擇 IMAX 廳的場次
      When 系統計算票價
      Then Ticket.price 為 380 元

  Rule: 訂單總金額為所有票券價格總和
    Example: 訂購 2 張一般數位廳票券
      Given 使用者訂購 2 張一般數位廳票券
      When 系統計算訂單總金額
      Then Order.total_price = 2 × 300 = 600 元

    Example: 訂購 3 張 IMAX 廳票券
      Given 使用者訂購 3 張 IMAX 廳票券
      When 系統計算訂單總金額
      Then Order.total_price = 3 × 380 = 1140 元

    Example: 無折扣機制
      Given 使用者訂購任意數量票券
      When 系統計算訂單總金額
      Then Order.total_price = Σ(Ticket.price)
      And 不套用任何折扣或優惠
