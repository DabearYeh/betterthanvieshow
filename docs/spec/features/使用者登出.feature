Feature: 使用者登出
  已登入的使用者可以登出系統

  Rule: 可以從導航列登出
    Example: 成功登出
      Given 使用者已登入
      When 使用者點擊導航列的「登出」按鈕
      Then 系統清除該使用者的登入狀態
      And 導向到首頁（未登入狀態）

  Rule: 登出後無法進行需登入的操作
    Example: 登出後無法訂票
      Given 使用者已登出
      When 使用者嘗試進入訂票功能
      Then 系統導向到登入頁面

  Rule: 登出後可以重新登入
    Example: 登出並重新登入
      Given 使用者已登出
      When 使用者重新輸入信箱與密碼登入
      Then 系統驗證通過
      And 重新建立 session
      And 導向到首頁
