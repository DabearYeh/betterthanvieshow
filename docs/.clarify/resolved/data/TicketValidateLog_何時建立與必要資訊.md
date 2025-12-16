# 釐清問題

TicketValidateLog 應在何時建立？應記錄哪些資訊？

# 定位

ERM：TicketValidateLog 實體
Feature：驗票.feature

# 多選題

| 選項 | 描述 |
|--------|-------------|
| A | 驗票成功時建立，記錄驗票時間與驗票人員；驗票失敗不記錄 |
| B | 驗票成功和失敗都記錄，另增加失敗原因欄位 |
| C | 只有驗票成功時建立，同時更新 Ticket status = 已入場 |

# 影響範圍

- TicketValidateLog 實體設計
- 驗票.feature 的後置條件
- 稽核與營運報表

# 優先級

Medium - 影響驗票功能的完整性

---
# 解決記錄

- **回答**：B - 驗票成功和失敗都記錄（簡化版，不記錄失敗原因）
- **更新的規格檔**：無需修改（當前設計已符合需求）
- **變更內容**：
  - 決策：每次驗票（無論成功或失敗）都建立 TicketValidateLog 記錄
  - 記錄內容：ticket_id、validated_at（驗票時間）、validated_by（驗票人員）、validation_result（成功/失敗）
  - 不記錄具體失敗原因（此決策與第 4 題「驗票_驗票失敗時是否需要記錄失敗原因」一致）
  - 當前 erm.dbml 中 TicketValidateLog 實體已包含 validation_result (bool) 欄位，符合此決策
