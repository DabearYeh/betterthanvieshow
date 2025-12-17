using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 使用者資料存取介面
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 檢查信箱是否已存在
    /// </summary>
    /// <param name="email">信箱</param>
    /// <returns>是否存在</returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// 建立新使用者
    /// </summary>
    /// <param name="user">使用者實體</param>
    /// <returns>建立後的使用者</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// 根據信箱取得使用者
    /// </summary>
    /// <param name="email">信箱</param>
    /// <returns>使用者實體或 null</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// 根據 ID 取得使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <returns>使用者實體或 null</returns>
    Task<User?> GetByIdAsync(int id);
}
