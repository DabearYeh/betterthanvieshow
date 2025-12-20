# 登入 API 實作完成

## 變更摘要

實作了 `POST /api/auth/login` 端點，支援會員登入功能。

### 新增檔案
- `LoginRequestDto.cs` - 登入請求 DTO
- `LoginResponseDto.cs` - 登入回應 DTO

### 修改檔案
- `IAuthService.cs` - 新增 `LoginAsync` 方法
- `AuthService.cs` - 實作登入邏輯
- `AuthController.cs` - 新增 `/login` 端點
- `JwtTokenGenerator.cs` - 修正配置路徑為 `JwtSettings`
- `Program.cs` - 修正配置路徑為 `JwtSettings`

---

## 測試結果

| 測試場景 | 狀態碼 | 回應訊息 | 結果 |
|---------|--------|---------|------|
| 登入成功 | 200 | `登入成功` + JWT Token | ✅ |
| 信箱不存在 | 401 | `信箱不存在` | ✅ |
| 密碼錯誤 | 401 | `密碼錯誤` | ✅ |

---

## 測試方法

在 `betterthanvieshow.http` 已新增三個測試請求：

```bash
dotnet run
```
然後在 `.http` 檔案中執行各測試請求。
