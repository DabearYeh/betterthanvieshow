# 會員註冊 API 實作計畫

為電影訂票系統建立一個完整的會員註冊 API，使用 Azure SQL Database 作為資料儲存。

## 需求分析

根據 [使用者註冊.feature](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/%E4%BD%BF%E7%94%A8%E8%80%85%E8%A8%BB%E5%86%8A.feature) 和 [erm.dbml](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/erm.dbml)，會員註冊功能需要滿足以下需求：

### 必填欄位
- **名稱** (`name`): 使用者姓名
- **信箱** (`email`): 登入帳號，必須唯一
- **密碼** (`password`): 需要加密儲存

### 驗證規則
1. **信箱唯一性**: 檢查信箱是否已被註冊
2. **密碼複雜度**: 至少 8 字元，包含大小寫字母與數字
3. **欄位必填**: name、email、password 都是必填項目

### 業務邏輯
- 註冊成功後預設角色為 `Customer`（顧客）
- 自動設定建立時間 (`created_at`)
- 註冊成功後自動登入
- `Admin` 角色由系統管理員手動設定

---

## 技術架構建議

### 技術棧選擇

> [!IMPORTANT]
> 在開始實作前，需要確認以下技術決策：
> 
> 1. **後端框架**: ASP.NET Core Web API / Node.js (Express) / Python (FastAPI/Django) / 其他？
> 2. **ORM 工具**: Entity Framework Core / Dapper / TypeORM / SQLAlchemy / 其他？
> 3. **驗證機制**: JWT / Session-based / OAuth2？
> 4. **密碼加密**: BCrypt / PBKDF2 / Argon2？

### 推薦方案 (以 ASP.NET Core 為例)

考慮到您使用 Azure SQL Database，推薦使用 **ASP.NET Core Web API** + **Entity Framework Core** 的組合：

- ✅ 與 Azure 生態系統整合良好
- ✅ Entity Framework Core 對 SQL Server/Azure SQL 支援完善
- ✅ 內建依賴注入、中介軟體、驗證授權機制
- ✅ 支援 JWT 認證
- ✅ 效能優異且可擴展

---

## API 設計規格

### 端點定義

#### POST `/api/auth/register`

**功能**: 註冊新會員

**請求 (Request)**

```json
{
  "name": "王小明",
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**成功回應 (200 OK)**

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
    "createdAt": "2025-12-17T00:21:40+08:00"
  }
}
```

**錯誤回應**

| 狀態碼 | 錯誤訊息 | 說明 |
|-------|---------|------|
| 400 Bad Request | `"名稱為必填欄位"` | 未提供名稱 |
| 400 Bad Request | `"信箱為必填欄位"` | 未提供信箱 |
| 400 Bad Request | `"密碼為必填欄位"` | 未提供密碼 |
| 400 Bad Request | `"信箱格式不正確"` | 信箱格式驗證失敗 |
| 400 Bad Request | `"密碼至少需 8 字元，包含大小寫字母與數字"` | 密碼複雜度不足 |
| 409 Conflict | `"此信箱已被使用"` | 信箱已存在 |
| 500 Internal Server Error | `"伺服器錯誤，請稍後再試"` | 系統內部錯誤 |

**錯誤回應格式**

```json
{
  "success": false,
  "message": "此信箱已被使用",
  "errors": {
    "email": ["此信箱已被使用"]
  }
}
```

---

## 資料模型與資料庫

### User 資料表結構 (基於 erm.dbml)

```sql
CREATE TABLE [User] (
    id INT PRIMARY KEY IDENTITY(1,1),
    email NVARCHAR(255) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    name NVARCHAR(100) NOT NULL,
    role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT CHK_User_Role CHECK (role IN ('Customer', 'Admin')),
    INDEX IX_User_Email (email)
);
```

### DTO 類別設計 (Data Transfer Objects)

#### RegisterRequestDto

```csharp
public class RegisterRequestDto
{
    [Required(ErrorMessage = "名稱為必填欄位")]
    [StringLength(100, ErrorMessage = "名稱長度不可超過 100 字元")]
    public string Name { get; set; }

    [Required(ErrorMessage = "信箱為必填欄位")]
    [EmailAddress(ErrorMessage = "信箱格式不正確")]
    [StringLength(255, ErrorMessage = "信箱長度不可超過 255 字元")]
    public string Email { get; set; }

    [Required(ErrorMessage = "密碼為必填欄位")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
        ErrorMessage = "密碼至少需 8 字元，包含大小寫字母與數字")]
    public string Password { get; set; }
}
```

#### RegisterResponseDto

```csharp
public class RegisterResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }  // JWT token
    public DateTime CreatedAt { get; set; }
}
```

---

## 實作流程

### 1. 控制器層 (Controller)

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "驗證失敗",
                Errors = ModelState.ToDictionary()
            });
        }
        
        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
        {
            return result.StatusCode == 409 
                ? Conflict(result) 
                : BadRequest(result);
        }
        
        return Ok(result);
    }
}
```

### 2. 服務層 (Service)

**IAuthService 介面**

```csharp
public interface IAuthService
{
    Task<ServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
}
```

**AuthService 實作**

```csharp
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    
    public async Task<ServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        // 1. 檢查信箱是否已存在
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return ServiceResult<RegisterResponseDto>.Failure(
                "此信箱已被使用", 
                409
            );
        }
        
        // 2. 建立使用者實體
        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            Password = _passwordHasher.HashPassword(request.Password),
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };
        
        // 3. 儲存到資料庫
        await _userRepository.CreateAsync(user);
        
        // 4. 產生 JWT token (自動登入)
        var token = _jwtTokenGenerator.GenerateToken(user);
        
        // 5. 回傳結果
        return ServiceResult<RegisterResponseDto>.Success(
            new RegisterResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                CreatedAt = user.CreatedAt
            }
        );
    }
}
```

### 3. 資料存取層 (Repository)

```csharp
public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> GetByEmailAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
    
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
```

---

## 安全性措施

### 1. 密碼加密

使用 **BCrypt** 或 **ASP.NET Core Identity PasswordHasher** 進行密碼雜湊：

```csharp
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
```

### 2. JWT Token 設定

```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])
        );
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 3. HTTPS 強制執行

在 `Program.cs` 中啟用：

```csharp
app.UseHttpsRedirection();
```

### 4. CORS 設定

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### 5. Rate Limiting (防止暴力破解)

使用 `AspNetCoreRateLimit` 套件限制註冊頻率：

```csharp
"IpRateLimiting": {
  "EnableEndpointRateLimiting": true,
  "GeneralRules": [
    {
      "Endpoint": "POST:/api/auth/register",
      "Period": "1h",
      "Limit": 5
    }
  ]
}
```

---

## Azure SQL Database 設定

### 連線字串配置

在 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=MovieTicketDB;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### DbContext 設定

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    // 其他 DbSet...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasDefaultValue("Customer");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });
    }
}
```

### Program.cs 註冊

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
```

---

## 錯誤處理與日誌

### 全域例外處理中介軟體

```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未處理的例外發生");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;
        
        var response = new ApiResponse
        {
            Success = false,
            Message = "伺服器錯誤，請稍後再試"
        };
        
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

### 日誌記錄

使用 **Serilog** 或 **Application Insights** (建議用於 Azure 環境):

```csharp
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]
);
```

---

## 驗證計畫

### 單元測試

```csharp
[Fact]
public async Task Register_WithValidData_ShouldReturnSuccess()
{
    // Arrange
    var request = new RegisterRequestDto
    {
        Name = "王小明",
        Email = "test@example.com",
        Password = "SecurePass123"
    };
    
    // Act
    var result = await _authService.RegisterAsync(request);
    
    // Assert
    Assert.True(result.Success);
    Assert.Equal("Customer", result.Data.Role);
    Assert.NotNull(result.Data.Token);
}

[Fact]
public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
{
    // Arrange
    var request = new RegisterRequestDto
    {
        Name = "王小明",
        Email = "existing@example.com",
        Password = "SecurePass123"
    };
    
    // Act
    var result = await _authService.RegisterAsync(request);
    
    // Assert
    Assert.False(result.Success);
    Assert.Equal(409, result.StatusCode);
    Assert.Equal("此信箱已被使用", result.Message);
}
```

### 整合測試

使用 Postman 或 REST Client 測試：

1. ✅ 成功註冊新會員
2. ✅ 信箱重複時回傳錯誤
3. ✅ 密碼複雜度不足時回傳錯誤
4. ✅ 必填欄位缺失時回傳錯誤
5. ✅ 註冊成功後可使用 token 存取受保護的端點

---

## 專案結構建議

```
MovieTicketAPI/
├── Controllers/
│   └── AuthController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IPasswordHasher.cs
│   │   └── IJwtTokenGenerator.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── PasswordHasher.cs
│       └── JwtTokenGenerator.cs
├── Repositories/
│   ├── Interfaces/
│   │   └── IUserRepository.cs
│   └── Implementations/
│       └── UserRepository.cs
├── Models/
│   ├── Entities/
│   │   └── User.cs
│   ├── DTOs/
│   │   ├── RegisterRequestDto.cs
│   │   └── RegisterResponseDto.cs
│   └── Responses/
│       ├── ApiResponse.cs
│       └── ServiceResult.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs
├── Migrations/
└── Program.cs
```

---

## 部署檢查清單

- [ ] 設定 Azure SQL Database 防火牆規則
- [ ] 將連線字串存放在 Azure Key Vault
- [ ] 啟用 Application Insights 監控
- [ ] 設定 CORS 允許的來源
- [ ] 執行資料庫遷移 (`dotnet ef database update`)
- [ ] 設定環境變數 (JWT SecretKey 等)
- [ ] 啟用 HTTPS
- [ ] 配置 Rate Limiting
- [ ] 部署到 Azure App Service
- [ ] 進行端對端測試

---

## 相關文件連結

- [使用者註冊功能規格](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/features/%E4%BD%BF%E7%94%A8%E8%80%85%E8%A8%BB%E5%86%8A.feature)
- [資料庫模型定義](file:///c:/Users/VivoBook/Desktop/betterthanvieshow/docs/spec/erm.dbml)
- [ASP.NET Core 官方文件](https://learn.microsoft.com/zh-tw/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/zh-tw/ef/core/)
- [Azure SQL Database](https://learn.microsoft.com/zh-tw/azure/azure-sql/)

---

## 後續擴充建議

1. **電子郵件驗證**: 註冊後發送驗證信確認信箱有效性
2. **OAuth 登入**: 支援 Google/Facebook/LINE 第三方登入
3. **雙因素認證 (2FA)**: 提升帳號安全性
4. **忘記密碼功能**: 透過信箱重設密碼
5. **使用者資料更新 API**: 允許修改個人資料
6. **軟刪除機制**: 保留使用者歷史記錄
