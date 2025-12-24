using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 電影資料存取實作
/// </summary>
public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _context;

    public MovieRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 建立新電影
    /// </summary>
    /// <param name="movie">電影實體</param>
    /// <returns>建立成功的電影實體</returns>
    public async Task<Movie> CreateAsync(Movie movie)
    {
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    /// <summary>
    /// 根據 ID 取得電影
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <returns>電影實體，若不存在回傳 null</returns>
    public async Task<Movie?> GetByIdAsync(int id)
    {
        return await _context.Movies.FindAsync(id);
    }

    /// <summary>
    /// 更新電影
    /// </summary>
    /// <param name="movie">電影實體</param>
    /// <returns>更新後的電影實體</returns>
    public async Task<Movie> UpdateAsync(Movie movie)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    /// <summary>
    /// 取得所有電影
    /// </summary>
    /// <returns>電影列表</returns>
    public async Task<List<Movie>> GetAllAsync()
    {
        return await _context.Movies
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
    }

    /// <summary>
    /// 取得輪播電影（CanCarousel = true）
    /// </summary>
    /// <returns>輪播電影列表</returns>
    public async Task<List<Movie>> GetCarouselMoviesAsync()
    {
        return await _context.Movies
            .Where(m => m.CanCarousel)
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
    }

    /// <summary>
    /// 取得即將上映電影（ReleaseDate > 今天）
    /// </summary>
    /// <returns>即將上映電影列表</returns>
    public async Task<List<Movie>> GetComingSoonMoviesAsync()
    {
        var today = DateTime.Today;
        return await _context.Movies
            .Where(m => m.ReleaseDate > today)
            .OrderBy(m => m.ReleaseDate)
            .ToListAsync();
    }

    /// <summary>
    /// 取得正在上映電影（今天在 [ReleaseDate, EndDate] 範圍內）
    /// </summary>
    /// <returns>正在上映電影列表</returns>
    public async Task<List<Movie>> GetMoviesOnSaleAsync()
    {
        var today = DateTime.Today;
        return await _context.Movies
            .Where(m => m.ReleaseDate <= today && m.EndDate >= today)
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
    }

    /// <summary>
    /// 取得最新建立的正在上映電影（按建立時間降序）
    /// </summary>
    /// <param name="count">取得數量</param>
    /// <returns>最新建立的正在上映電影列表</returns>
    public async Task<List<Movie>> GetRecentOnSaleMoviesAsync(int count)
    {
        var today = DateTime.Today;
        return await _context.Movies
            .Where(m => m.ReleaseDate <= today && m.EndDate >= today)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// 搜尋電影（根據關鍵字搜尋標題）
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <returns>符合條件的電影列表</returns>
    public async Task<List<Movie>> SearchMoviesAsync(string keyword)
    {
        var today = DateTime.Today;
        var lowerKeyword = keyword.ToLower();

        return await _context.Movies
            .Where(m => 
                // 只搜尋正在上映或即將上映的電影
                (m.ReleaseDate <= today && m.EndDate >= today) || m.ReleaseDate > today
            )
            .Where(m => 
                // 只搜尋標題
                m.Title.ToLower().Contains(lowerKeyword)
            )
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
    }
}

