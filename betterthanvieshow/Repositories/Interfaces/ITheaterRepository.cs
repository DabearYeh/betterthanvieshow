using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 影廳資料存取介面
/// </summary>
public interface ITheaterRepository
{
    /// <summary>
    /// 取得所有影廳
    /// </summary>
    /// <returns>影廳實體列表</returns>
    Task<List<Theater>> GetAllAsync();

    /// <summary>
    /// 建立新影廳
    /// </summary>
    /// <param name="theater">影廳實體</param>
    /// <returns>建立成功的影廳實體</returns>
    Task<Theater> CreateAsync(Theater theater);

    /// <summary>
    /// 批次建立座位並更新影廳的 TotalSeats
    /// </summary>
    /// <param name="theaterId">影廳 ID</param>
    /// <param name="seats">座位列表</param>
    /// <param name="totalSeats">座位總數</param>
    Task CreateSeatsAsync(int theaterId, List<Seat> seats, int totalSeats);

    /// <summary>
    /// 批次建立座位（不更新 TotalSeats）
    /// </summary>
    /// <param name="theaterId">影廳 ID</param>
    /// <param name="seats">座位列表</param>
    Task CreateSeatsOnlyAsync(int theaterId, List<Seat> seats);

    /// <summary>
    /// 根據 ID 取得影廳
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>影廳實體</returns>
    Task<Theater> GetByIdAsync(int id);

    /// <summary>
    /// 根據 ID 取得影廳及其所有座位
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>影廳實體（包含座位），若不存在回傳 null</returns>
    Task<Theater?> GetByIdWithSeatsAsync(int id);

    /// <summary>
    /// 檢查影廳是否存在
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>存在回傳 true，否則回傳 false</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 檢查影廳是否有關聯的場次
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>有場次回傳 true，否則回傳 false</returns>
    Task<bool> HasShowtimesAsync(int id);

    /// <summary>
    /// 刪除影廳及其所有座位
    /// </summary>
    /// <param name="id">影廳 ID</param>
    Task DeleteAsync(int id);
}
