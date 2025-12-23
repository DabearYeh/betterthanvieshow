Feature: 設定場次
  影城管理者可以設定電影場次

  Rule: 必須指定電影
    Example: 成功指定電影
      Given 管理者進入設定場次頁面
      When 管理者選擇電影「復仇者聯盟」
      Then 系統記錄選擇的電影

  Rule: 必須指定影廳
    Example: 成功指定影廳
      Given 管理者已選擇電影
      When 管理者選擇影廳「IMAX 廳」
      Then 系統記錄選擇的影廳

  Rule: 必須指定放映日期
    Example: 成功指定放映日期
      Given 管理者已選擇電影與影廳
      When 管理者選擇放映日期「2025-12-20」
      Then 系統記錄放映日期

  Rule: 必須指定放映時間
    Example: 成功指定放映時間
      Given 管理者已選擇電影、影廳、日期
      When 管理者輸入放映時間「14:00」
      Then 系統記錄放映時間

  Rule: 同一影廳同一日期同一時間只能有一個場次
    Example: 新場次時間不衝突，允許建立
      Given 影廳「廳 A」已有場次「2025-12-20 14:00」
      When 管理者新增場次「2025-12-20 16:30」
      Then 系統建立 MovieShowTime 記錄

    Example: 新場次時間衝突，禁止建立
      Given 影廳「廳 A」已有場次「2025-12-20 14:00」
      When 管理者新增場次「2025-12-20 14:00」
      Then 操作失敗

  Rule: 場次設定完成後保存
    Example: 場次設定成功
      Given 管理者完成所有場次資訊
      When 管理者按下確認設定鈕
      Then 系統建立 MovieShowTime 記錄

  Rule: 只有草稿狀態的日期可以設定場次
    Example: 草稿狀態可新增場次
      Given 日期「2025-12-22」的時刻表狀態為 Draft
      When 管理者在日期「2025-12-22」新增場次
      Then 系統建立 MovieShowTime 記錄

    Example: 販售中狀態禁止新增場次
      Given 日期「2025-12-22」的時刻表狀態為 OnSale
      When 管理者嘗試在日期「2025-12-22」新增場次
      Then 操作失敗

  Rule: 新增場次時自動建立或關聯每日時刻表
    Example: 日期首次新增場次時建立 DailySchedule
      Given 日期「2025-12-25」尚無時刻表記錄
      When 管理者在日期「2025-12-25」新增場次
      Then 系統建立 DailySchedule 記錄
      And 該 DailySchedule 的狀態為 Draft
      And 系統建立 MovieShowTime 記錄

    Example: 日期已有時刻表時關聯現有記錄
      Given 日期「2025-12-22」已有時刻表記錄（狀態為 Draft）
      When 管理者在日期「2025-12-22」新增第二個場次
      Then 系統不重複建立 DailySchedule
      And 系統建立 MovieShowTime 記錄並關聯至現有時刻表
