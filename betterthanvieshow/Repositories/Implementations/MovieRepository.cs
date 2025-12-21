using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;

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
}
