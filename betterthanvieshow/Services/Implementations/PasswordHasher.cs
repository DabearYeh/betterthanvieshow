using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 密碼加密服務實作（使用 BCrypt）
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// 將明文密碼進行雜湊加密
    /// </summary>
    public string HashPassword(string password)
    {
        // 使用 BCrypt 進行加密，workFactor 12 提供良好的安全性
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// 驗證密碼是否正確
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
