Feature: 驗票
  管理者可以驗證票券有效性

  Rule: 必須檢查票券是否存在
    Example: 票券存在時驗票成功
      Given 管理者掃描有效的票券 QR Code
      When 系統查詢票券編號
      Then 系統找到對應票券記錄

    Example: 票券不存在時驗票失敗
      Given 管理者掃描無效的 QR Code
      When 系統查詢票券編號
      Then 系統回傳「票券不存在」

  Rule: 票券必須是未使用狀態
    Example: 未使用票券可以驗票
      Given 票券狀態為「未使用」
      When 管理者驗票
      Then 系統允許驗票

    Example: 已使用票券無法重複驗票
      Given 票券狀態為「已使用」
      When 管理者試圖驗票
      Then 系統回傳「票券已使用」
      
    Example: 已過期票券無法驗票
      Given 票券狀態為「已過期」
      When 管理者試圖驗票
      Then 系統回傳「票券已過期」

  Rule: 管理者身份自動識別
    Example: 驗票人員身份使用登入使用者 ID
      Given 管理者已登入系統
      When 管理者驗票成功
      Then 系統自動將當前登入使用者 ID 寫入 TicketValidateLog.validated_by
      And 無需手動輸入管理者身份

  Rule: 驗票失敗時回饋具體原因
    Example: 票券不存在時回饋對應訊息
      Given 管理者掃描不存在的 QR Code
      When 系統進行驗票
      Then 系統回傳「票券不存在」

    Example: 票券已使用時回饋對應訊息
      Given 票券狀態為「已使用」
      When 管理者試圖驗票
      Then 系統回傳「票券已使用」

    Example: 票券已過期時回饋對應訊息
      Given 票券狀態為「已過期」
      When 管理者試圖驗票
      Then 系統回傳「票券已過期」

  Rule: 每次驗票嘗試都必須建立記錄
    Example: 驗票成功建立記錄
      Given 驗票成功
      When 系統完成驗票
      Then 系統建立 TicketValidateLog 記錄
      And validation_result 為 true
      And 記錄驗票時間與人員

    Example: 驗票失敗建立記錄
      Given 驗票失敗（票券無效/已使用/已過期）
      When 系統完成驗票
      Then 系統建立 TicketValidateLog 記錄
      And validation_result 為 false
      And 記錄驗票時間與人員
