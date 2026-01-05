# Bug 修復文件

此資料夾用於存放專案中所有 bug 修復的相關文件。

## 文件命名規範

建議使用以下格式命名：
- `YYYY-MM-DD-bug-description.md` - 例如：`2026-01-04-enum-validation-fix.md`
- 或使用 Issue 編號：`issue-123-description.md`

## 文件內容建議

每個 bug 修復文件應包含：
1. **問題描述** - Bug 的詳細說明
2. **重現步驟** - 如何觸發這個 bug
3. **根本原因** - 為什麼會發生這個問題
4. **修復方案** - 如何解決這個問題
5. **修改的檔案** - 列出所有修改的檔案
6. **測試驗證** - 如何驗證修復是否成功

## 目錄

- [2026-01-05 - Orders API 修改為只返回已付款訂單](./2026-01-05-orders-paid-only-filter/README.md)
- [2026-01-05 - Theater API 新增 canDelete 欄位](./2026-01-05-theater-candelete-field/README.md)
- [2026-01-04 - Enum 驗證錯誤處理修復](./2026-01-04-enum-validation-controller-fix/2026-01-04-enum-validation-controller-fix.md)
