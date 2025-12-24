using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Models.Responses;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 電影服務實作
/// </summary>
public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<MovieService> _logger;

    public MovieService(
        IMovieRepository movieRepository,
        ILogger<MovieService> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }

    /// <summary>
    /// 建立新電影
    /// </summary>
    /// <param name="request">建立電影請求</param>
    /// <returns>建立結果</returns>
    public async Task<ApiResponse<MovieResponseDto>> CreateMovieAsync(CreateMovieRequestDto request)
    {
        try
        {
            _logger.LogInformation("開始建立電影: {Title}", request.Title);

            // 驗證下映日期必須大於等於上映日期
            if (request.EndDate < request.ReleaseDate)
            {
                _logger.LogWarning("下映日期 {EndDate} 小於上映日期 {ReleaseDate}", 
                    request.EndDate, request.ReleaseDate);
                return ApiResponse<MovieResponseDto>.FailureResponse(
                    "下映日期必須大於或等於上映日期");
            }

            // 建立電影實體
            var movie = new Movie
            {
                Title = request.Title,
                Description = request.Description,
                Duration = request.Duration,
                Genre = request.Genre,
                Rating = request.Rating,
                Director = request.Director,
                Cast = request.Cast,
                PosterUrl = request.PosterUrl,
                TrailerUrl = request.TrailerUrl,
                ReleaseDate = request.ReleaseDate,
                EndDate = request.EndDate,
                CanCarousel = request.CanCarousel,
                CreatedAt = DateTime.UtcNow
            };

            // 儲存電影
            var createdMovie = await _movieRepository.CreateAsync(movie);

            _logger.LogInformation("成功建立電影: {Title}, ID: {Id}", 
                createdMovie.Title, createdMovie.Id);

            // 轉換為回應 DTO
            var responseDto = new MovieResponseDto
            {
                Id = createdMovie.Id,
                Title = createdMovie.Title,
                Description = createdMovie.Description,
                Duration = createdMovie.Duration,
                Genre = createdMovie.Genre,
                Rating = createdMovie.Rating,
                Director = createdMovie.Director,
                Cast = createdMovie.Cast,
                PosterUrl = createdMovie.PosterUrl,
                TrailerUrl = createdMovie.TrailerUrl,
                ReleaseDate = createdMovie.ReleaseDate,
                EndDate = createdMovie.EndDate,
                CanCarousel = createdMovie.CanCarousel,
                CreatedAt = createdMovie.CreatedAt
            };

            return ApiResponse<MovieResponseDto>.SuccessResponse(responseDto, "電影建立成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立電影時發生錯誤: {Title}", request.Title);
            return ApiResponse<MovieResponseDto>.FailureResponse($"建立電影時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新電影
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <param name="request">更新電影請求</param>
    /// <returns>更新結果</returns>
    public async Task<ApiResponse<MovieResponseDto>> UpdateMovieAsync(int id, UpdateMovieRequestDto request)
    {
        try
        {
            _logger.LogInformation("開始更新電影: ID={Id}", id);

            // 檢查電影是否存在
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                _logger.LogWarning("找不到電影: ID={Id}", id);
                return ApiResponse<MovieResponseDto>.FailureResponse("找不到指定的電影");
            }

            // 驗證下映日期必須大於等於上映日期
            if (request.EndDate < request.ReleaseDate)
            {
                _logger.LogWarning("下映日期 {EndDate} 小於上映日期 {ReleaseDate}", 
                    request.EndDate, request.ReleaseDate);
                return ApiResponse<MovieResponseDto>.FailureResponse(
                    "下映日期必須大於或等於上映日期");
            }

            // 更新電影欄位
            movie.Title = request.Title;
            movie.Description = request.Description;
            movie.Duration = request.Duration;
            movie.Genre = request.Genre;
            movie.Rating = request.Rating;
            movie.Director = request.Director;
            movie.Cast = request.Cast;
            movie.PosterUrl = request.PosterUrl;
            movie.TrailerUrl = request.TrailerUrl;
            movie.ReleaseDate = request.ReleaseDate;
            movie.EndDate = request.EndDate;
            movie.CanCarousel = request.CanCarousel;

            // 儲存更新
            var updatedMovie = await _movieRepository.UpdateAsync(movie);

            _logger.LogInformation("成功更新電影: {Title}, ID: {Id}", 
                updatedMovie.Title, updatedMovie.Id);

            // 轉換為回應 DTO
            var responseDto = new MovieResponseDto
            {
                Id = updatedMovie.Id,
                Title = updatedMovie.Title,
                Description = updatedMovie.Description,
                Duration = updatedMovie.Duration,
                Genre = updatedMovie.Genre,
                Rating = updatedMovie.Rating,
                Director = updatedMovie.Director,
                Cast = updatedMovie.Cast,
                PosterUrl = updatedMovie.PosterUrl,
                TrailerUrl = updatedMovie.TrailerUrl,
                ReleaseDate = updatedMovie.ReleaseDate,
                EndDate = updatedMovie.EndDate,
                CanCarousel = updatedMovie.CanCarousel,
                CreatedAt = updatedMovie.CreatedAt
            };

            return ApiResponse<MovieResponseDto>.SuccessResponse(responseDto, "電影更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新電影時發生錯誤: ID={Id}", id);
            return ApiResponse<MovieResponseDto>.FailureResponse($"更新電影時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 取得所有電影
    /// </summary>
    /// <returns>電影列表</returns>
    public async Task<ApiResponse<List<MovieListItemDto>>> GetAllMoviesAsync()
    {
        try
        {
            _logger.LogInformation("開始取得所有電影");

            var movies = await _movieRepository.GetAllAsync();
            var today = DateTime.UtcNow.Date;

            var movieList = movies.Select(m => new MovieListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                PosterUrl = m.PosterUrl,
                Duration = m.Duration,
                Rating = m.Rating,
                ReleaseDate = m.ReleaseDate,
                EndDate = m.EndDate,
                Status = GetMovieStatus(m.ReleaseDate, m.EndDate, today)
            }).ToList();

            _logger.LogInformation("成功取得 {Count} 部電影", movieList.Count);

            return ApiResponse<List<MovieListItemDto>>.SuccessResponse(movieList, "取得電影列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得電影列表時發生錯誤");
            return ApiResponse<List<MovieListItemDto>>.FailureResponse($"取得電影列表時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 取得單一電影詳情
    /// </summary>
    /// <param name="id">電影 ID</param>
    /// <returns>電影詳情</returns>
    public async Task<ApiResponse<MovieResponseDto>> GetMovieByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("開始取得電影詳情: ID={Id}", id);

            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                _logger.LogWarning("找不到電影: ID={Id}", id);
                return ApiResponse<MovieResponseDto>.FailureResponse("找不到指定的電影");
            }

            var responseDto = new MovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Duration = movie.Duration,
                Genre = movie.Genre,
                Rating = movie.Rating,
                Director = movie.Director,
                Cast = movie.Cast,
                PosterUrl = movie.PosterUrl,
                TrailerUrl = movie.TrailerUrl,
                ReleaseDate = movie.ReleaseDate,
                EndDate = movie.EndDate,
                CanCarousel = movie.CanCarousel,
                CreatedAt = movie.CreatedAt
            };

            _logger.LogInformation("成功取得電影詳情: {Title}, ID: {Id}", 
                movie.Title, movie.Id);

            return ApiResponse<MovieResponseDto>.SuccessResponse(responseDto, "取得電影詳情成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得電影詳情時發生錯誤: ID={Id}", id);
            return ApiResponse<MovieResponseDto>.FailureResponse($"取得電影詳情時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 取得首頁電影資料（輪播、本週前10、即將上映、隨機推薦、所有電影）
    /// </summary>
    /// <returns>首頁電影資料</returns>
    public async Task<ApiResponse<HomepageMoviesResponseDto>> GetHomepageMoviesAsync()
    {
        try
        {
            _logger.LogInformation("開始取得首頁電影資料");

            // 順序取得各類別電影資料（避免 EF Core DbContext 並發問題）
            var carouselMovies = await _movieRepository.GetCarouselMoviesAsync();
            var topWeeklyMovies = await _movieRepository.GetRecentOnSaleMoviesAsync(10);
            var comingSoonMovies = await _movieRepository.GetComingSoonMoviesAsync();
            var onSaleMovies = await _movieRepository.GetMoviesOnSaleAsync();


            // 隨機推薦：從正在上映的電影中隨機選 10 部
            var recommendedMovies = onSaleMovies
                .OrderBy(_ => Random.Shared.Next())
                .Take(10)
                .ToList();

            // 所有電影：正在上映 + 即將上映
            var allMovies = onSaleMovies
                .Concat(comingSoonMovies)
                .OrderByDescending(m => m.ReleaseDate)
                .ToList();

            // 轉換為 DTO
            var response = new HomepageMoviesResponseDto
            {
                Carousel = carouselMovies.Select(MapToSimpleDto).ToList(),
                TopWeekly = topWeeklyMovies.Select(MapToSimpleDto).ToList(),
                ComingSoon = comingSoonMovies.Select(MapToSimpleDto).ToList(),
                Recommended = recommendedMovies.Select(MapToSimpleDto).ToList(),
                AllMovies = allMovies.Select(MapToSimpleDto).ToList()
            };

            _logger.LogInformation(
                "成功取得首頁電影資料 - 輪播: {CarouselCount}, 本週前10: {TopWeeklyCount}, " +
                "即將上映: {ComingSoonCount}, 隨機推薦: {RecommendedCount}, 所有電影: {AllMoviesCount}",
                response.Carousel.Count, response.TopWeekly.Count, response.ComingSoon.Count,
                response.Recommended.Count, response.AllMovies.Count);

            return ApiResponse<HomepageMoviesResponseDto>.SuccessResponse(response, "取得首頁電影資料成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得首頁電影資料時發生錯誤");
            return ApiResponse<HomepageMoviesResponseDto>.FailureResponse($"取得首頁電影資料時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 將 Movie 實體轉換為 MovieSimpleDto
    /// </summary>
    private static MovieSimpleDto MapToSimpleDto(Movie movie)
    {
        return new MovieSimpleDto
        {
            Id = movie.Id,
            Title = movie.Title,
            PosterUrl = movie.PosterUrl,
            Duration = movie.Duration,
            Genre = movie.Genre,
            Rating = movie.Rating,
            ReleaseDate = movie.ReleaseDate,
            EndDate = movie.EndDate
        };
    }

    /// <summary>
    /// 搜尋電影
    /// </summary>
    /// <param name="keyword">搜尋關鍵字</param>
    /// <returns>搜尋結果</returns>
    public async Task<ApiResponse<List<MovieSimpleDto>>> SearchMoviesAsync(string keyword)
    {
        try
        {
            _logger.LogInformation("搜尋電影: {Keyword}", keyword);

            // 驗證關鍵字
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _logger.LogWarning("搜尋關鍵字為空");
                return ApiResponse<List<MovieSimpleDto>>.FailureResponse("搜尋關鍵字不可為空");
            }

            // 搜尋電影
            var movies = await _movieRepository.SearchMoviesAsync(keyword.Trim());

            // 轉換為 DTO
            var result = movies.Select(MapToSimpleDto).ToList();

            _logger.LogInformation("搜尋電影完成，找到 {Count} 部電影", result.Count);

            return ApiResponse<List<MovieSimpleDto>>.SuccessResponse(
                result, 
                $"找到 {result.Count} 部符合的電影"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜尋電影時發生錯誤: {Keyword}", keyword);
            return ApiResponse<List<MovieSimpleDto>>.FailureResponse(
                $"搜尋電影時發生錯誤: {ex.Message}"
            );
        }
    }

    /// <summary>
    /// 計算電影上映狀態
    /// </summary>
    private static string GetMovieStatus(DateTime releaseDate, DateTime endDate, DateTime today)

    {
        if (today < releaseDate.Date)
            return "即將上映";
        if (today <= endDate.Date)
            return "上映中";
        return "已下映";
    }
}
