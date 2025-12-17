# Azure SQL Database 連線設定完成

✅ **狀態**: 所有設定已完成，API 正在運行中

---

## 已完成的工作

### 1. 更新 appsettings.json

✅ **連線字串設定**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:betterthanvieshow-sql.database.windows.net,1433;Initial Catalog=BetterThanVieShowWebAppDB;..."
  }
}
```

✅ **JWT 安全密鑰**
- 已產生 64 字元隨機密鑰

### 2. Azure 防火牆設定

✅ **允許本機 IP**: `36.238.11.186`

### 3. 資料庫遷移

✅ **執行結果**:
```
Build started...
Build succeeded.
Done.
```

**已建立的資料表**: `User`
- ✅ 主鍵 `id` (IDENTITY)
- ✅ 唯一索引 `email`
- ✅ 角色檢查約束 (Customer/Admin)
- ✅ 預設值設定

### 4. API 服務啟動

✅ **運行狀態**: 正在運行
- HTTPS: `https://localhost:7255`
- HTTP: `http://localhost:5041`

---

## API 測試指南

### 使用 PowerShell 測試

#### 測試 1: 註冊新會員

```powershell
$registerBody = @{
    name = "測試用戶"
    email = "test@example.com"
    password = "SecurePass123"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:7255/api/auth/register" `
    -Method Post `
    -Body $registerBody `
    -ContentType "application/json" `
    -SkipCertificateCheck

$response | ConvertTo-Json -Depth 10
```

**預期結果**:
```json
{
  "success": true,
  "message": "註冊成功",
  "data": {
    "userId": 1,
    "name": "測試用戶",
    "email": "test@example.com",
    "role": "Customer",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "createdAt": "2025-12-16T17:16:50Z"
  }
}
```

#### 測試 2: 驗證信箱唯一性

```powershell
# 再次使用相同信箱註冊
$response = Invoke-RestMethod `
    -Uri "https://localhost:7255/api/auth/register" `
    -Method Post `
    -Body $registerBody `
    -ContentType "application/json" `
    -SkipCertificateCheck `
    -SkipHttpErrorCheck

$response | ConvertTo-Json
```

**預期結果**: HTTP 409 Conflict
```json
{
  "success": false,
  "message": "此信箱已被使用",
  "errors": {
    "email": ["此信箱已被使用"]
  }
}
```

#### 測試 3: 驗證密碼複雜度

```powershell
$weakPasswordBody = @{
    name = "弱密碼測試"
    email = "weak@example.com"
    password = "123"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:7255/api/auth/register" `
    -Method Post `
    -Body $weakPasswordBody `
    -ContentType "application/json" `
    -SkipCertificateCheck `
    -SkipHttpErrorCheck

$response | ConvertTo-Json
```

**預期結果**: HTTP 400 Bad Request
```json
{
  "success": false,
  "message": "驗證失敗",
  "errors": {
    "Password": ["密碼至少需 8 字元，包含大小寫字母與數字"]
  }
}
```

---

## 資料庫驗證

如果您想直接查看資料庫中的資料，可以使用 Azure Data Studio 或 SSMS 連線：

**連線資訊**:
- Server: `betterthanvieshow-sql.database.windows.net`
- Database: `BetterThanVieShowWebAppDB`
- Authentication: SQL Login
- Username: `betterthanvieshow`

**查詢範例**:
```sql
-- 查看所有註冊的使用者
SELECT * FROM [User];

-- 查看所有顧客
SELECT * FROM [User] WHERE role = 'Customer';

-- 統計使用者數量
SELECT COUNT(*) as TotalUsers FROM [User];
```

---

## 已設定的安全措施

- ✅ **密碼加密**: BCrypt (workFactor: 12)
- ✅ **JWT 認證**: HS256 演算法，7 天有效期
- ✅ **HTTPS 強制執行**: 所有請求自動重導向至 HTTPS
- ✅ **CORS 設定**: 允許跨域請求
- ✅ **輸入驗證**: 
  - 名稱最多 100 字元
  - 信箱格式驗證
  - 密碼複雜度要求（至少 8 字元，含大小寫字母與數字）
- ✅ **資料庫層級**: 
  - Email 唯一性約束
  - 角色檢查約束 (Customer/Admin)

---

## 下一步建議

### 立即可做

1. **測試 API**: 使用上述測試指南驗證功能
2. **開發前端**: 建立註冊頁面與 API 整合
3. **實作登入功能**: 建立 `POST /api/auth/login` 端點

### 功能擴充

1. **電子郵件驗證**: 註冊後發送確認信
2. **忘記密碼**: 透過信箱重設密碼
3. **使用者資料管理**: 個人資料 CRUD API
4. **OAuth 整合**: Google/Facebook/LINE 登入
5. **訂票功能**: 根據 erm.dbml 實作訂票流程

### 安全性強化

1. **Rate Limiting**: 防止暴力破解（ASP.NET Core Rate Limiting）
2. **Refresh Token**: 實作 Token 刷新機制
3. **雙因素認證**: 提升帳號安全性
4. **日誌監控**: 整合 Application Insights

---

## 相關文件

- [會員註冊 API 使用指南](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/會員註冊API使用指南.md)
- [Azure 防火牆設定指南](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/plans/Azure防火牆設定指南.md)
- [功能規格](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/%E4%BD%BF%E7%94%A8%E8%80%85%E8%A8%BB%E5%86%8A.feature)
- [資料庫模型](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/erm.dbml)

---

## 問題排查

### API 無法啟動

如果 `dotnet run` 失敗：
1. 確認連線字串正確
2. 檢查防火牆規則
3. 驗證 JWT SecretKey 設定

### 無法連線資料庫

1. 確認 IP 在防火牆規則中
2. 測試連線字串
3. 檢查 Azure SQL Database 狀態

### 測試時收到 500 錯誤

1. 查看終端機的錯誤日誌
2. 檢查資料庫連線
3. 驗證密碼加密設定

---

**🎉 恭喜！您的會員註冊 API 已完全設定完成並可以使用了！**
