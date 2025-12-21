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
}
