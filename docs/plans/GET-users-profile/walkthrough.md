# 取得使用者個人資料 API 實作步驟

## 狀態：已完成 ✅

我們已經完成了取得使用者個人資料 API 的實作。

### 1. 建立 DTO
[UserProfileResponseDto.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Models/DTOs/UserProfileResponseDto.cs)
定義了回傳的資料結構：`Id`, `Name`, `Email`。

### 2. 建立 Service 層
- 介面：[IUserService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Interfaces/IUserService.cs)
- 實作：[UserService.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Services/Implementations/UserService.cs)
  - 封裝了從 Repository 取得資料並轉換為 DTO 的邏輯。
  - 使用 `IUserRepository` 獲取 User 實體。

### 3. 配置依賴注入
在 [Program.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Program.cs) 中註冊了 `IUserService`。

```csharp
builder.Services.AddScoped<IUserService, UserService>();
```

### 4. 建立 Controller
[UsersController.cs](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/betterthanvieshow/Controllers/UsersController.cs)
- 路由：`GET /api/users/profile`
- 權限：`[Authorize]`
- 邏輯：從 JWT Token (Claims) 中解析 `UserId`，呼叫 Service 取得資料。

---

## 測試指南

### 1. 取得 Token
先呼叫登入 API 取得 Token：
```http
POST http://localhost:5041/api/auth/login
Content-Type: application/json

{
  "email": "wang@example.com",
  "password": "Password123!"
}
```

### 2. 測試 Profile API
將 Token 放入 Header：
```http
GET http://localhost:5041/api/users/profile
Authorization: Bearer <把Token貼在這裡>
```

### 預期結果 (200 OK)
```json
{
  "success": true,
  "message": "取得個人資料成功",
  "data": {
    "id": 1,
    "name": "王小明",
    "email": "wang@example.com"
  }
}
```
