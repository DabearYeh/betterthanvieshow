Feature: 瀏覽場次
  前台使用者可以瀏覽電影的可選場次

  Rule: 查看電影的所有場次
    Example: 顯示電影的場次列表
      Given 使用者選擇電影「復仇者聯盟」
      When 使用者查看此電影的場次
      Then 系統顯示該電影的所有上映場次

  Rule: 場次顯示影廳資訊
    Example: 場次包含影廳名稱和類型
      Given 場次列表已顯示
      When 使用者查看場次詳情
      Then 系統顯示影廳名稱和影廳類型（一般數位、4DX、IMAX）

  Rule: 場次顯示放映時間
    Example: 顯示放映日期和時間
      Given 場次列表已顯示
      When 使用者查看場次詳情
      Then 系統顯示放映日期和開始時間

  Rule: 場次顯示可用座位數
    Example: 顯示座位可用情況
      Given 場次「2025-12-20 14:00」有 100 個座位
      When 已售出 20 張票
      Then 系統顯示可用座位數為 80
