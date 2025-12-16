Feature: 編輯影廳
  影城管理者可以編輯影廳資訊

  Rule: 可以修改影廳名稱
    Example: 成功修改影廳名稱
      Given 影廳「廳 A」已建立
      When 管理者修改影廳名稱為「IMAX 廳」
      Then 系統更新影廳名稱為「IMAX 廳」

  Rule: 可以修改影廳類型
    Example: 成功修改影廳類型
      Given 影廳類型為「一般數位」
      When 管理者修改影廳類型為「4DX」
      Then 系統更新影廳類型為「4DX」

  Rule: 可以修改影廳樓層
    Example: 成功修改影廳樓層
      Given 影廳所在樓層為「2」
      When 管理者修改影廳樓層為「3」
      Then 系統更新影廳樓層為「3」

  Rule: 座位配置後排列不可改動
    Example: 排列數無法修改
      Given 影廳已設定座位配置（排數為 10）
      When 管理者嘗試修改排數
      Then 系統不允許修改（排列已設定）
