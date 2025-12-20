# 登入 API 實作計畫

實作會員登入 API `POST /api/auth/login`，根據 `使用者登入.feature` 規格。

## 功能需求摘要

根據規格與上傳的 UI 圖片：
- 輸入：信箱與密碼
- **信箱不存在**：顯示「信箱不存在」
- **密碼錯誤**：顯示「密碼錯誤」
- **登入成功**：回傳 JWT Token 與使用者資訊

## Proposed Changes

### DTOs

#### [NEW] LoginRequestDto.cs
登入請求 DTO，包含：
- `Email` - 必填，Email 格式驗證
- `Password` - 必填

---

#### [NEW] LoginResponseDto.cs
登入回應 DTO，包含：
- `UserId`、`Name`、`Email`、`Role`、`Token`

---

### Service Layer

#### [MODIFY] IAuthService.cs
新增方法簽名：
```csharp
Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
```

---

#### [MODIFY] AuthService.cs
實作 `LoginAsync` 邏輯：
1. 用 `_userRepository.GetByEmailAsync()` 查詢使用者
2. 若不存在，回傳「信箱不存在」
3. 用 `_passwordHasher.VerifyPassword()` 驗證密碼
4. 若不符，回傳「密碼錯誤」
5. 產生 JWT Token 並回傳使用者資訊

---

### Controller Layer

#### [MODIFY] AuthController.cs
新增 endpoint：
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
```

回應狀態碼：
- `200 OK` - 登入成功
- `401 Unauthorized` - 信箱不存在或密碼錯誤

---

## Verification Plan

### HTTP 檔案測試

在 `betterthanvieshow.http` 中新增測試請求：

```http
### 登入成功
POST {{host}}/api/Auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test1234"
}

### 信箱不存在
POST {{host}}/api/Auth/login
Content-Type: application/json

{
  "email": "notexist@example.com",
  "password": "Test1234"
}

### 密碼錯誤
POST {{host}}/api/Auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "WrongPassword123"
}
```

### 測試步驟

1. 執行 `dotnet run` 啟動應用程式
2. 先執行註冊 API 建立測試帳號
3. 依序測試三個場景，驗證回應狀態碼和訊息
