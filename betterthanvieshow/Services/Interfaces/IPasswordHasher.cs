namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 密碼加密服務介面
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 將明文密碼進行雜湊加密
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <returns>加密後的密碼</returns>
    string HashPassword(string password);

    /// <summary>
    /// 驗證密碼是否正確
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <param name="hashedPassword">已加密的密碼</param>
    /// <returns>是否匹配</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
