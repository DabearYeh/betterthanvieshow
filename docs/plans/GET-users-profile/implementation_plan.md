# 實作取得使用者個人資料 API

## 目標
實作 `GET /api/users/profile` 端點，讓已登入的使用者取得自己的個人資料（姓名、Email）。

## 背景
使用者需要一個介面來查看自己的個人資訊（如設定頁面）。
參考設計圖：顯示使用者頭像（目前暫無）、姓名、Email。

## 變更內容

### 1. DTO
新增 `UserProfileResponseDto`：
```csharp
public class UserProfileResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

### 2. Service
新增 `IUserService` 與 `UserService`：
- 方法：`Task<ApiResponse<UserProfileResponseDto>> GetUserProfileAsync(int userId);`
- 邏輯：
    1. 呼叫 `_userRepository.GetByIdAsync(userId)`
    2. 若找不到，回傳失敗
    3. 轉換為 DTO 並回傳

### 3. Controller
新增 `UsersController`：
- 路由：`api/users`
- 端點：`GET profile`
- 權限：`[Authorize]` (所有登入使用者)
- 邏輯：從 User.Claims 取得 UserId，呼叫 Service。

### 4. 註冊服務
在 `Program.cs` 註冊 `IUserService` / `UserService`。

## 驗證
1. 登入取得 Token。
2. 使用 Token 呼叫 `GET /api/users/profile`。
3. 驗證回傳的 Name 和 Email 是否正確。
