# 會員註冊 API 實作完成

✅ **專案狀態**: 已成功完成會員註冊 API 的實作

## 已完成的工作

### 1. 專案結構建立

建立了完整的三層架構：

```
betterthanvieshow/
├── Models/
│   ├── Entities/
│   │   └── User.cs                          # 使用者實體
│   ├── DTOs/
│   │   ├── RegisterRequestDto.cs            # 註冊請求 DTO
│   │   └── RegisterResponseDto.cs           # 註冊回應 DTO
│   └── Responses/
│       └── ApiResponse.cs                   # 統一 API 回應格式
├── Data/
│   └── ApplicationDbContext.cs              # EF Core DbContext
├── Repositories/
│   ├── Interfaces/
│   │   └── IUserRepository.cs               # Repository 介面
│   └── Implementations/
│       └── UserRepository.cs                # Repository 實作
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs                  # 認證服務介面
│   │   ├── IPasswordHasher.cs               # 密碼加密介面
│   │   └── IJwtTokenGenerator.cs            # JWT Token 介面
│   └── Implementations/
│       ├── AuthService.cs                   # 認證服務實作
│       ├── PasswordHasher.cs                # BCrypt 密碼加密
│       └── JwtTokenGenerator.cs             # JWT Token 生成器
├── Controllers/
│   └── AuthController.cs                    # 認證控制器
├── Migrations/
│   └── [timestamp]_InitialCreate.cs         # 資料庫遷移檔
├── Program.cs                               # 應用程式入口點
└── appsettings.json                         # 設定檔
```

### 2. 已安裝的 NuGet 套件

- ✅ `Microsoft.EntityFrameworkCore.SqlServer` (9.0.0)
- ✅ `Microsoft.EntityFrameworkCore.Tools` (9.0.0)
- ✅ `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.0)
- ✅ `BCrypt.Net-Next` (4.0.3)

### 3. 核心功能

#### 會員註冊 API

**端點**: `POST /api/auth/register`

**請求範例**:
```json
{
  "name": "王小明",
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**成功回應** (200 OK):
```json
{
  "success": true,
  "message": "註冊成功",
  "data": {
    "userId": 1,
    "name": "王小明",
    "email": "user@example.com",
    "role": "Customer",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "createdAt": "2025-12-16T16:29:42Z"
  }
}
```

**錯誤回應** (409 Conflict - 信箱已存在):
```json
{
  "success": false,
  "message": "此信箱已被使用",
  "errors": {
    "email": ["此信箱已被使用"]
  }
}
```

**錯誤回應** (400 Bad Request - 密碼不符合規則):
```json
{
  "success": false,
  "message": "驗證失敗",
  "errors": {
    "Password": ["密碼至少需 8 字元，包含大小寫字母與數字"]
  }
}
```

### 4. 安全措施

- 🔒 **密碼加密**: 使用 BCrypt (workFactor: 12) 進行密碼雜湊
- 🔑 **JWT 認證**: 7 天有效期，HS256 演算法
- ✉️ **信箱唯一性**: 資料庫層級的唯一索引
- ✔️ **輸入驗證**: 
  - 名稱長度最多 100 字元
  - 信箱格式驗證
  - 密碼至少 8 字元，必須包含大小寫字母與數字
- 🌐 **CORS**: 已配置允許跨域請求
- 🔐 **HTTPS**: 強制使用 HTTPS 重導向

---

## 設定與部署指南

### 步驟 1: 設定 Azure SQL Database 連線字串

編輯 [`appsettings.json`](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:您的伺服器名稱.database.windows.net,1433;Initial Catalog=MovieTicketDB;User ID=您的使用者名稱;Password=您的密碼;Encrypt=True;TrustServerCertificate=False;"
  }
}
```

> [!IMPORTANT]
> 請將連線字串中的以下內容替換為您的實際 Azure SQL Database 資訊：
> - `您的伺服器名稱`: Azure SQL Server 名稱
> - `您的使用者名稱`: 資料庫登入帳號
> - `您的密碼`: 資料庫密碼

### 步驟 2: 設定 JWT 密鑰

> [!WARNING]
> **生產環境安全性**: 請務必更改預設的 JWT SecretKey！

在 `appsettings.json` 中更新 JWT 設定：

```json
{
  "Jwt": {
    "SecretKey": "請更改為至少32個字元的隨機密鑰",
    "Issuer": "BetterThanVieShowAPI",
    "Audience": "BetterThanVieShowClient"
  }
}
```

**生成安全的 SecretKey (PowerShell)**:
```powershell
# 生成 64 字元的隨機密鑰
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]$_})
```

### 步驟 3: 執行資料庫遷移

在專案目錄執行以下命令：

```powershell
# 確認 Azure SQL Database 防火牆已允許您的 IP
dotnet ef database update
```

這會在 Azure SQL Database 中建立 `User` 資料表，包含：
- 主鍵 `id`
- 唯一索引 `email`
- 角色檢查約束 (Customer/Admin)
- 預設值設定

### 步驟 4: 執行專案

```powershell
dotnet run
```

應用程式會在以下地址啟動：
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`

API 文件可透過 Scalar UI 查看：
- 開發環境: `https://localhost:7xxx/scalar/v1`

---

## 測試指南

### 使用 PowerShell 測試

#### 測試 1: 成功註冊

```powershell
$body = @{
    name = "王小明"
    email = "test@example.com"
    password = "SecurePass123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7xxx/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$response | ConvertTo-Json -Depth 10
```

**預期結果**: 
- HTTP 200 OK
- 回傳使用者資訊與 JWT Token

#### 測試 2: 重複信箱

```powershell
# 使用相同信箱再次註冊
$response = Invoke-RestMethod -Uri "https://localhost:7xxx/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck `
    -StatusCodeVariable statusCode `
    -SkipHttpErrorCheck

Write-Host "Status: $statusCode"
$response | ConvertTo-Json
```

**預期結果**:
- HTTP 409 Conflict
- 錯誤訊息: "此信箱已被使用"

#### 測試 3: 密碼複雜度不足

```powershell
$weakBody = @{
    name = "測試用戶"
    email = "weak@example.com"
    password = "123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7xxx/api/auth/register" `
    -Method Post `
    -Body $weakBody `
    -ContentType "application/json" `
    -SkipCertificateCheck `
    -SkipHttpErrorCheck

$response | ConvertTo-Json
```

**預期結果**:
- HTTP 400 Bad Request
- 錯誤訊息: "密碼至少需 8 字元，包含大小寫字母與數字"

### 使用 Postman / Thunder Client 測試

1. **新增請求**:
   - Method: `POST`
   - URL: `https://localhost:7xxx/api/auth/register`
   - Headers: `Content-Type: application/json`

2. **Body (raw JSON)**:
   ```json
   {
     "name": "測試用戶",
     "email": "test@example.com",
     "password": "SecurePass123"
   }
   ```

3. **驗證回應**:
   - ✅ 檢查 `success: true`
   - ✅ 檢查 `data.token` 存在
   - ✅ 檢查 `data.role` 為 "Customer"

---

## 驗證結果

### ✅ 專案編譯

```
✓ dotnet build
  betterthanvieshow net9.0 成功 (1.8 秒)
```

### ✅ 資料庫遷移

```
✓ dotnet ef migrations add InitialCreate
  Build succeeded.
  Done. To undo this action, use 'ef migrations remove'
```

### ✅ 套件版本一致性

所有 .NET 9.0 專案相關套件統一為 9.0.0 版本：
- Entity Framework Core: 9.0.0
- ASP.NET Core Authentication: 9.0.0
- dotnet-ef CLI 工具: 9.0.0

---

## 資料表結構

執行遷移後，Azure SQL Database 會建立以下結構：

```sql
CREATE TABLE [User] (
    id INT PRIMARY KEY IDENTITY(1,1),
    email NVARCHAR(255) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    name NVARCHAR(100) NOT NULL,
    role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT CHK_User_Role CHECK (role IN ('Customer', 'Admin'))
);

CREATE UNIQUE INDEX IX_User_Email ON [User](email);
```

---

## 後續工作建議

### 立即可做

1. **測試 API**: 使用上述測試指南驗證 API 功能
2. **部署到 Azure App Service**: 發布應用程式到雲端
3. **設定 Azure Key Vault**: 安全儲存敏感設定

### 功能擴充

1. **登入 API**: 實作 `POST /api/auth/login`
2. **電子郵件驗證**: 註冊後發送確認信
3. **忘記密碼**: 透過信箱重設密碼
4. **使用者資料管理**: CRUD API for User Profile
5. **OAuth 整合**: Google/Facebook/LINE 登入

### 安全性強化

1. **Rate Limiting**: 防止暴力破解
2. **Refresh Token**: 實作 Token 更新機制
3. **雙因素認證 (2FA)**: 提升帳號安全性
4. **密碼歷史**: 防止重複使用舊密碼

---

## 相關文件

- [實作計畫](file:///C:/Users/VivoBook/.gemini/antigravity/brain/247b9833-41b1-4019-82b3-5f703a465779/implementation_plan.md)
- [功能規格](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/%E4%BD%BF%E7%94%A8%E8%80%85%E8%A8%BB%E5%86%8A.feature)
- [資料庫模型](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/erm.dbml)
- [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs)
- [AuthController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/AuthController.cs)
- [AuthService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/AuthService.cs)

---

## 問題排查

### 連線失敗

如果無法連線到 Azure SQL Database：

1. **檢查防火牆規則**: 在 Azure Portal 新增您的 IP 地址
2. **驗證連線字串**: 確認伺服器名稱、資料庫名稱、帳號密碼正確
3. **測試連線**: 使用 Azure Data Studio 或 SSMS 測試連線

### JWT Token 無效

如果 Token 驗證失敗：

1. **確認 SecretKey**: Issuer/Audience 設定一致
2. **檢查 Token 有效期**: Token 預設 7 天有效
3. **時鐘同步**: 確保伺服器時間正確

### 密碼驗證失敗

如果登入時密碼驗證錯誤：

1. **BCrypt 版本**: 確認使用 BCrypt.Net-Next 4.0.3+
2. **WorkFactor**: 預設為 12，確保一致性

---

## 聯絡支援

如有問題或需要協助，請參考：
- [ASP.NET Core 文件](https://learn.microsoft.com/zh-tw/aspnet/core/)
- [Entity Framework Core 文件](https://learn.microsoft.com/zh-tw/ef/core/)
- [Azure SQL Database 文件](https://learn.microsoft.com/zh-tw/azure/azure-sql/)
