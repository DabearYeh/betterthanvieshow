Feature: 刪除場次
  影城管理者可以在草稿狀態下刪除場次
  販售中狀態的場次無法刪除

  Background:
    Given 管理者已登入系統

  Rule: 草稿狀態的場次可以刪除
    Example: 成功刪除草稿狀態的場次
      Given 日期「2025-12-22」的時刻表狀態為 Draft
      And 場次「誅仙 - 2025/12/22 09:00」已存在
      When 管理者刪除該場次
      Then 該場次被刪除
      And 系統刪除對應的 MovieShowTime 記錄

  Rule: 販售中狀態的場次無法刪除
    Example: 販售中狀態禁止刪除場次
      Given 日期「2025-12-22」的時刻表狀態為 OnSale
      And 場次「誅仙 - 2025/12/22 09:00」已存在
      When 管理者嘗試刪除該場次
      Then 操作失敗
      And 場次仍然存在

  Rule: 刪除最後一個場次時保留時刻表記錄
    Example: 刪除唯一場次後時刻表仍存在
      Given 日期「2025-12-22」的時刻表狀態為 Draft
      And 該日期只有一個場次「誅仙 - 09:00」
      When 管理者刪除該場次
      Then 該場次被刪除
      And 日期「2025-12-22」的時刻表記錄仍存在
      And 時刻表狀態維持 Draft
