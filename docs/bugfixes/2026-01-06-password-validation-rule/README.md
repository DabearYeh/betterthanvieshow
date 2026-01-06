# 密碼驗證規則修改

## 📅 修改日期
2026-01-06

## 🎯 修改目的
簡化會員註冊的密碼驗證規則，從原本的 8 碼（需包含大小寫字母與數字）降低為 6 碼（只需包含英文字母或數字）。

## 📋 問題背景

### 原有規則
- **密碼長度**：至少 8 字元
- **複雜度要求**：必須包含大寫字母、小寫字母和數字
- **正則表達式**：`^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$`
- **錯誤訊息**：「密碼至少需 8 字元，包含大小寫字母與數字」

### 修改需求
- **密碼長度**：至少 6 字元
- **字元限制**：只能包含英文字母（大小寫）和數字
- **允許情況**：可以是純數字、純英文，或英數混合
- **不允許**：中文、特殊符號、空格等

## 🔧 修改內容

### 1. RegisterRequestDto.cs

**檔案位置**：`betterthanvieshow/Models/DTOs/RegisterRequestDto.cs`

**修改前**：
```csharp
[Required(ErrorMessage = "密碼為必填欄位")]
[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
    ErrorMessage = "密碼至少需 8 字元，包含大小寫字母與數字")]
public string Password { get; set; } = string.Empty;
```

**修改後**：
```csharp
[Required(ErrorMessage = "密碼為必填欄位")]
[RegularExpression(@"^[a-zA-Z0-9]{6,}$",
    ErrorMessage = "密碼至少需 6 字元，且只能包含英文字母與數字")]
public string Password { get; set; } = string.Empty;
```

### 2. AuthController.cs

**檔案位置**：`betterthanvieshow/Controllers/AuthController.cs`

**修改內容**：
- 更新註冊 API 文檔中的密碼範例：`Password123!` → `pass12`
- 更新錯誤訊息範例：「密碼長度需大於 6 碼」→「密碼至少需 6 字元，且只能包含英文字母與數字」

## 🎯 新密碼規則

### 正則表達式
```regex
^[a-zA-Z0-9]{6,}$
```

### 規則說明
- `^` - 字串開始
- `[a-zA-Z0-9]` - 只允許英文字母（大小寫）和數字
- `{6,}` - 至少 6 個字元，無上限
- `$` - 字串結束

### 有效密碼範例
✅ `123456` - 純數字，6 字元
✅ `password` - 純英文，8 字元
✅ `pass12` - 英文+數字，6 字元
✅ `ABC123` - 大寫英文+數字，6 字元
✅ `Password1` - 混合大小寫+數字，9 字元

### 無效密碼範例
❌ `pass` - 只有 4 字元（少於 6）
❌ `12345` - 只有 5 字元（少於 6）
❌ `pass@123` - 包含特殊符號 @
❌ `我的密碼` - 包含中文
❌ `pass 12` - 包含空格

## ⚠️ 重要提醒

### 對現有帳號的影響
**不會影響！** 原因如下：

1. **密碼儲存機制**：
   - 密碼使用 BCrypt 進行雜湊加密後儲存
   - 資料庫中儲存的是**雜湊值**，不是明文密碼

2. **登入驗證機制**：
   - 登入時使用 `BCrypt.Verify()` 比對雜湊值
   - **不檢查原始密碼的格式或長度**
   - 只要雜湊值匹配即可登入

3. **結論**：
   - ✅ 舊帳號（8碼複雜密碼）：仍可正常登入
   - ✅ 新帳號（6碼簡單密碼）：使用新規則創建
   - ✅ 完全向下相容

## 📝 相關檔案

- `betterthanvieshow/Models/DTOs/RegisterRequestDto.cs`
- `betterthanvieshow/Controllers/AuthController.cs`
- `betterthanvieshow/Services/Implementations/PasswordHasher.cs`（未修改，僅供參考）

## 🔄 Git 資訊

- **分支**：`bugfix/password-validation`
- **基於**：`main` 分支

## 📊 測試建議

建議測試以下情境：

1. **新註冊帳號**：
   - 使用 6 碼純數字密碼註冊
   - 使用 6 碼純英文密碼註冊
   - 使用英數混合密碼註冊

2. **驗證錯誤訊息**：
   - 嘗試使用 5 碼密碼（應顯示錯誤）
   - 嘗試使用包含特殊符號的密碼（應顯示錯誤）
   - 嘗試使用包含中文的密碼（應顯示錯誤）

3. **舊帳號登入**：
   - 確認現有帳號仍可正常登入
