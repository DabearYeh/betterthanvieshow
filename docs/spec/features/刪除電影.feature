Feature: 刪除電影
  影城管理者可以下架電影

  Rule: 電影不能被刪除，只能上架或下架
    Example: 下架電影
      Given 電影「復仇者聯盟」已上架
      When 管理者下架此電影
      Then 系統設定 Movie.is_published 為假
      And 電影不再於前台顯示

    Example: 上架電影
      Given 電影「復仇者聯盟」已下架
      When 管理者上架此電影
      Then 系統設定 Movie.is_published 為真
      And 電影於前台顯示
