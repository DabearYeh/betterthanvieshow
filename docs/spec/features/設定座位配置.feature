Feature: 設定座位配置
  影城管理者可以設定影廳的座位排列方式

  Rule: 先選擇排數和列數生成網格
    Example: 生成座位網格
      Given 管理者正在設定影廳座位配置
      When 管理者設定排數「10」、列數「12」
      Then 系統生成 10×12 的網格（共 120 個格子）

  Rule: 對每個格子標記座位類型
    Example: 標記一般座位
      Given 座位網格已生成
      When 管理者點擊某格子並選擇「一般座位」
      Then 系統標記該格子為一般座位

    Example: 標記殘障座位
      Given 座位網格已生成
      When 管理者點擊某格子並選擇「殘障座位」
      Then 系統標記該格子為殘障座位

    Example: 標記走道
      Given 座位網格已生成
      When 管理者點擊某格子並選擇「走道」
      Then 系統標記該格子為走道（不計入座位）

    Example: 標記 Empty
      Given 座位網格已生成
      When 管理者點擊某格子並選擇「Empty」
      Then 系統標記該格子為 Empty（不屬於座位區，不會售票，不在計數裡）

  Rule: 系統自動計算總座位數
    Example: 計算座位總數
      Given 管理者已標記所有格子
      When 系統計算座位總數（一般座 + 殘障座）
      Then 系統自動更新 Theater.total_seats
      And 與用戶標記的座位數一致

  Rule: 座位預設為有效
    Example: 完成設定後座位有效
      Given 管理者完成所有座位標記
      When 管理者按下確認鈕
      Then 系統建立所有 Seat 記錄
      And 所有座位 is_valid 預設為真
