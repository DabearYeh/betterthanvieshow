using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 使用者服務實作
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<UserProfileResponseDto>> GetUserProfileAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("找不到使用者: ID={UserId}", userId);
                return ApiResponse<UserProfileResponseDto>.FailureResponse("找不到使用者");
            }

            var response = new UserProfileResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return ApiResponse<UserProfileResponseDto>.SuccessResponse(response, "取得個人資料成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得個人資料時發生錯誤: UserID={UserId}", userId);
            return ApiResponse<UserProfileResponseDto>.FailureResponse("取得個人資料時發生錯誤");
        }
    }
}
