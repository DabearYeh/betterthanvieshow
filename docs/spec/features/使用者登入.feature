Feature: 使用者登入
  註冊過的使用者可以登入系統

  Rule: 必須輸入信箱
    Example: 成功輸入信箱
      Given 使用者在登入頁面
      When 使用者輸入信箱「user@example.com」
      Then 系統準備驗證

  Rule: 必須輸入密碼
    Example: 成功輸入密碼
      Given 使用者已輸入信箱
      When 使用者輸入密碼「SecurePass123」
      Then 系統準備驗證

  Rule: 信箱與密碼必須匹配
    Example: 密碼正確時允許登入
      Given 系統中存在 User（email: user@example.com, password: 已加密儲存）
      When 使用者輸入正確的信箱與密碼
      Then 系統驗證通過，建立 session
      And 導向到首頁

    Example: 密碼錯誤時拒絕登入
      Given 系統中存在 User（email: user@example.com）
      When 使用者輸入正確信箱但錯誤密碼
      Then 系統提示「信箱或密碼錯誤」

  Rule: 信箱不存在時拒絕登入
    Example: 未註冊的信箱無法登入
      Given 系統中不存在 User（email: nonexistent@example.com）
      When 使用者嘗試用此信箱登入
      Then 系統提示「信箱或密碼錯誤」

  Rule: 登入成功後建立 session
    Example: 登入後可進行後續操作
      Given 使用者登入成功
      When 使用者進入首頁
      Then 系統識別該使用者已登入
      And 可進行訂票等操作
