Feature: 編輯電影
  影城管理者可以編輯電影資訊

  Rule: 可修改片名
    Example: 成功修改片名
      Given 電影「復仇者聯盟」已存在
      When 管理者修改片名為「復仇者聯盟2」
      Then 系統更新電影片名為「復仇者聯盟2」

  Rule: 可修改簡介
    Example: 成功修改簡介
      Given 電影簡介已存在
      When 管理者修改簡介為「新的劇情介紹...」
      Then 系統更新電影簡介

  Rule: 可修改時長
    Example: 成功修改時長
      Given 電影時長為 120 分鐘
      When 管理者修改時長為 130 分鐘
      Then 系統更新電影時長為 130 分鐘

  Rule: 修改時長後必須大於 0
    Example: 時長不能修改為 0
      Given 電影時長為 120 分鐘
      When 管理者修改時長為 0
      Then 操作失敗

  Rule: 可修改導演
    Example: 修改導演
      Given 電影導演為「李安」
      When 管理者修改導演為「侯孝賢」
      Then 系統更新電影導演

  Rule: 可修改演員
    Example: 修改演員
      Given 電影演員已設定
      When 管理者修改演員資訊
      Then 系統更新電影演員

  Rule: 可修改電影分級
    Example: 修改電影分級
      Given 電影分級為「普遍級」
      When 管理者修改電影分級為「輔導級」
      Then 系統更新電影分級

  Rule: 可修改影片類型
    Example: 修改影片類型
      Given 電影類型為「動作」
      When 管理者修改類型為「動作、科幻」
      Then 系統更新電影類型

  Rule: 可修改輪播設定
    Example: 修改輪播設定
      Given 電影的輪播設定為假
      When 管理者勾選「加入輪播」
      Then 系統更新 Movie.can_carousel 為真

  Rule: 可修改放映日期
    Example: 修改放映日期
      Given 電影放映日期為「2025-12-20」
      When 管理者修改放映日期為「2025-12-25」
      Then 系統更新電影放映日期

  Rule: 可修改海報
    Example: 修改海報
      Given 電影海報已設定
      When 管理者上傳新的海報
      Then 系統更新電影海報 URL

  Rule: 可修改預告片聯結
    Example: 修改預告片聯結
      Given 電影預告片聯結已設定
      When 管理者輸入新的預告片聯結
      Then 系統更新預告片 URL
