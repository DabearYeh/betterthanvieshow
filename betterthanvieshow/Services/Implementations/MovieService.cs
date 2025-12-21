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
}
