using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// JWT Token 生成器介面
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// 為使用者生成 JWT Token
    /// </summary>
    /// <param name="user">使用者實體</param>
    /// <returns>JWT Token 字串</returns>
    string GenerateToken(User user);
}
