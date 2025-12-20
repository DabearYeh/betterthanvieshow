using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 認證服務實作
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <summary>
    /// 會員註冊
    /// </summary>
    public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        // 1. 檢查信箱是否已存在
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return ApiResponse<RegisterResponseDto>.FailureResponse(
                "此信箱已被使用",
                new Dictionary<string, string[]>
                {
                    { "email", new[] { "此信箱已被使用" } }
                }
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
        var response = new RegisterResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<RegisterResponseDto>.SuccessResponse(response, "註冊成功");
    }

    /// <summary>
    /// 會員登入
    /// </summary>
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        // 1. 查詢使用者
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLower());
        
        if (user == null)
        {
            return ApiResponse<LoginResponseDto>.FailureResponse(
                "信箱不存在",
                new Dictionary<string, string[]>
                {
                    { "email", new[] { "信箱不存在" } }
                }
            );
        }

        // 2. 驗證密碼
        if (!_passwordHasher.VerifyPassword(request.Password, user.Password))
        {
            return ApiResponse<LoginResponseDto>.FailureResponse(
                "密碼錯誤",
                new Dictionary<string, string[]>
                {
                    { "password", new[] { "密碼錯誤" } }
                }
            );
        }

        // 3. 產生 JWT Token
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. 回傳結果
        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Token = token
        };

        return ApiResponse<LoginResponseDto>.SuccessResponse(response, "登入成功");
    }
}
