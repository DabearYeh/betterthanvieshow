Feature: 瀏覽電影
  前台使用者可以瀏覽電影資訊

  Rule: 顯示本週前十電影
    # 本週前十：過去 7 天內銷售最多的前 10 部已上映電影（按已付款訂單計算，每日更新）
    Example: 查看本週前十列表
      Given 使用者進入首頁
      When 使用者查看「本週前十」欄位
      Then 系統顯示過去 7 天內銷售最多前 10 部已上架電影
      And 排名於每日凌晨更新一次
      And 銷售計數：該期間內已完成付款的訂單所含票券

  Rule: 顯示即將上映電影
    # 即將上映：放映日 > 今天 且 is_published = true
    Example: 查看即將上映列表
      Given 使用者進入首頁
      When 使用者查看「即將上映」欄位
      Then 系統顯示所有 release_date > 今天的上架電影

  Rule: 只顯示已上架的電影
    Example: 下架電影不顯示
      Given 電影「舊電影」已下架（is_published = false）
      When 使用者瀏覽電影列表
      Then 系統不顯示下架電影

  Rule: 輪播電影自動顯示
    Example: 有輪播標籤的電影在首頁輪播
      Given 電影「熱映電影」的 can_carousel = 真
      When 使用者進入首頁
      Then 系統在輪播區域顯示此電影
