Feature: 刪除影廳
  影廳不能被刪除，系統不提供刪除功能

  Rule: 影廳無法被刪除
    Example: 系統不提供影廳刪除功能
      Given 影廳「廳 A」已建立
      When 管理者查看影廳管理頁面
      Then 系統不提供刪除影廳的功能
      And 影廳無法被刪除
