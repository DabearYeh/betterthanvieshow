Feature: 建立電影
  影城管理者可以建立電影資訊

  Rule: 必須有片名
    Example: 成功輸入片名
      Given 管理者正在建立新電影
      When 管理者輸入片名「復仇者聯盟」
      Then 系統記錄電影片名為「復仇者聯盟」

  Rule: 必須有簡介
    Example: 成功輸入簡介
      Given 管理者已輸入片名
      When 管理者輸入簡介「英雄集結，拯救世界的故事...」
      Then 系統記錄電影簡介

  Rule: 必須有時長
    Example: 成功輸入時長
      Given 管理者輸入片名、簡介
      When 管理者輸入時長「120」
      Then 系統記錄電影時長為 120 分鐘

  Rule: 時長必須大於 0
    Example: 時長不能為 0
      Given 管理者完成電影基本資訊
      When 管理者輸入時長「0」
      Then 操作失敗

  Rule: 可選設定導演
    Example: 設定導演
      Given 管理者完成電影基本資訊
      When 管理者輸入導演「李安」
      Then 系統記錄電影導演

  Rule: 可選設定演員
    Example: 設定演員
      Given 管理者完成電影基本資訊
      When 管理者輸入演員「張三、李四」
      Then 系統記錄電影演員

  Rule: 可選設定電影分級
    Example: 設定電影分級
      Given 管理者完成電影基本資訊
      When 管理者選擇電影分級「輔導級」
      Then 系統記錄電影分級為「輔導級」

  Rule: 可選選擇影片類型
    Example: 選擇影片類型
      Given 管理者完成電影基本資訊
      When 管理者勾選類型「動作」、「科幻」
      Then 系統記錄電影類型（可多選）

  Rule: 可選擇是否加入輪播
    Example: 勾選加入輪播
      Given 管理者完成電影基本資訊
      When 管理者勾選「加入輪播」
      Then 系統記錄 Movie.can_carousel 為真

  Rule: 可選設定放映日期
    Example: 設定放映日期
      Given 管理者完成電影基本資訊
      When 管理者設定放映日期為「2025-12-20」
      Then 系統記錄 Movie.release_date 為「2025-12-20」

  Rule: 可選上傳海報
    Example: 上傳海報
      Given 管理者完成電影基本資訊
      When 管理者上傳海報
      Then 系統記錄電影海報 URL

  Rule: 可選上傳預告片聯結
    Example: 上傳預告片聯結
      Given 管理者完成電影基本資訊
      When 管理者輸入預告片聯結「https://example.com/trailer」
      Then 系統記錄預告片 URL

  Rule: 成功建立後電影上架
    Example: 建立完成自動上架
      Given 管理者完成所有電影資訊（至少片名、簡介、時長）
      When 管理者按下確認建立鈕
      Then 系統建立 Movie 記錄
      And 系統設定 Movie.is_published 為真
