Feature: 建立影廳
  影城管理者可以建立影廳

  Rule: 必須設定影廳名稱
    Example: 成功設定影廳名稱
      Given 管理者正在建立新影廳
      When 管理者輸入影廳名稱「廳 A」
      Then 系統記錄影廳名稱為「廳 A」

  Rule: 必須設定影廳類型
    Example: 成功選擇影廳類型
      Given 管理者已輸入影廳名稱
      When 管理者選擇影廳類型「IMAX」
      Then 系統記錄影廳類型為「IMAX」

  Rule: 必須設定樓層
    Example: 成功設定樓層
      Given 管理者已選擇影廳類型
      When 管理者輸入所在樓層「2」
      Then 系統記錄影廳樓層為「2」

  Rule: 必須設定排數和列數
    Example: 成功設定排數和列數
      Given 管理者已輸入影廳基本資訊（名稱、類型、樓層）
      When 管理者設定排數「10」、列數「12」
      Then 系統生成 10×12 的座位網格

  Rule: 排數和列數必須大於 0
    Example: 排數或列數不能為 0
      Given 管理者正在設定座位配置
      When 管理者輸入排數「0」
      Then 操作失敗

  Rule: 建立影廳後需設定座位配置
    Example: 建立完成後導向座位配置
      Given 管理者完成影廳基本資訊
      When 管理者按下確認建立鈕
      Then 系統建立 Theater 記錄
      And 系統導向「設定座位配置」功能
