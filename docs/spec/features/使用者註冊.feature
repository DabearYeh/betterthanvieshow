Feature: 使用者註冊
  新使用者可以建立帳號

  Rule: 必須輸入名稱
    Example: 成功輸入名稱
      Given 使用者在註冊頁面
      When 使用者輸入名稱「王小明」
      Then 系統記錄使用者名稱為「王小明」

  Rule: 必須輸入信箱
    Example: 成功輸入信箱
      Given 使用者已輸入名稱
      When 使用者輸入信箱「user@example.com」
      Then 系統記錄 User.email

  Rule: 信箱必須唯一
    Example: 信箱已存在時提示
      Given 信箱「existing@example.com」已被註冊
      When 使用者嘗試用相同信箱註冊
      Then 系統提示「此信箱已被使用」

  Rule: 必須輸入密碼
    Example: 成功輸入密碼
      Given 使用者已輸入名稱與信箱
      When 使用者輸入密碼「SecurePass123」
      Then 系統以加密方式儲存密碼

  Rule: 密碼必須符合複雜度要求
    Example: 密碼過於簡單時提示
      Given 使用者輸入密碼「123」
      When 系統檢驗密碼
      Then 系統提示「密碼至少需 8 字元，包含大小寫字母與數字」

  Rule: 註冊成功後自動登入
    Example: 註冊後導向首頁
      Given 使用者完成註冊（名稱、信箱、密碼）
      When 使用者按下確認註冊鈕
      Then 系統建立 User 記錄
      And 系統自動登入該使用者
      And 導向到首頁

  Rule: 註冊時預設角色為顧客
    Example: 新註冊使用者為顧客角色
      Given 使用者完成註冊
      When 系統建立 User 記錄
      Then User.role 設為「Customer」（顧客）
      And 該使用者可訂票、查詢自己的訂票記錄、瀏覽電影/場次

    Example: 管理者角色由系統管理員手動設定
      Given 系統需要新增管理者
      When 系統管理員手動修改 User.role
      Then 可設為「Admin」（管理者）
      And Admin 可管理電影、影廳、場次、座位配置，查詢所有訂票記錄與執行驗票
