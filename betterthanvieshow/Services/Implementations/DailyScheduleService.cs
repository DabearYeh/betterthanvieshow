using betterthanvieshow.Data;
using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 每日時刻表服務實作
/// </summary>
public class DailyScheduleService : IDailyScheduleService
{
    private readonly ApplicationDbContext _context;
    private readonly IDailyScheduleRepository _dailyScheduleRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ITheaterRepository _theaterRepository;

    public DailyScheduleService(
        ApplicationDbContext context,
        IDailyScheduleRepository dailyScheduleRepository,
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        ITheaterRepository theaterRepository)
    {
        _context = context;
        _dailyScheduleRepository = dailyScheduleRepository;
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _theaterRepository = theaterRepository;
    }

    /// <inheritdoc />
    public async Task<DailyScheduleResponseDto> SaveDailyScheduleAsync(DateTime date, SaveDailyScheduleRequestDto dto)
    {
        // 正規化日期（只保留日期部分）
        var scheduleDate = date.Date;

        // 開啟交易
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. 檢查或建立 DailySchedule
            var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(scheduleDate);

            if (dailySchedule == null)
            {
                // 不存在 → 建立新的（Draft 狀態）
                dailySchedule = await _dailyScheduleRepository.CreateAsync(new DailySchedule
                {
                    ScheduleDate = scheduleDate,
                    Status = "Draft"
                });
            }
            else if (dailySchedule.Status == "OnSale")
            {
                // 已販售 → 禁止修改
                throw new InvalidOperationException("該日期的時刻表已開始販售，無法修改");
            }

            // 2. 驗證所有場次資料
            var showtimesToCreate = new List<MovieShowTime>();
            var movieCache = new Dictionary<int, Movie>(); // 快取電影資料
            var theaterCache = new Dictionary<int, bool>(); // 快取影廳存在狀態

            foreach (var item in dto.Showtimes)
            {
                // 驗證電影存在
                if (!movieCache.ContainsKey(item.MovieId))
                {
                    var movie = await _movieRepository.GetByIdAsync(item.MovieId);
                    if (movie == null)
                    {
                        throw new ArgumentException($"電影 ID {item.MovieId} 不存在");
                    }
                    movieCache[item.MovieId] = movie;
                }

                var movieData = movieCache[item.MovieId];

                // 驗證影廳存在
                if (!theaterCache.ContainsKey(item.TheaterId))
                {
                    var exists = await _theaterRepository.ExistsAsync(item.TheaterId);
                    if (!exists)
                    {
                        throw new ArgumentException($"影廳 ID {item.TheaterId} 不存在");
                    }
                    theaterCache[item.TheaterId] = true;
                }

                // 解析時間
                if (!TimeSpan.TryParse(item.StartTime, out var startTime))
                {
                    throw new ArgumentException($"時間格式錯誤：{item.StartTime}");
                }

                // 驗證 15 分鐘倍數
                if (startTime.Minutes % 15 != 0)
                {
                    throw new ArgumentException($"開始時間必須是 15 分鐘的倍數：{item.StartTime}");
                }

                // 驗證放映日期在上映範圍內
                if (scheduleDate < movieData.ReleaseDate.Date || scheduleDate > movieData.EndDate.Date)
                {
                    throw new ArgumentException(
                        $"電影「{movieData.Title}」的放映日期必須在 {movieData.ReleaseDate:yyyy-MM-dd} 至 {movieData.EndDate:yyyy-MM-dd} 之間");
                }

                showtimesToCreate.Add(new MovieShowTime
                {
                    MovieId = item.MovieId,
                    TheaterId = item.TheaterId,
                    ShowDate = scheduleDate,
                    StartTime = startTime
                });
            }

            // 3. 檢查場次清單內部的時間衝突
            CheckInternalTimeConflicts(showtimesToCreate, movieCache);

            // 4. 全刪全建
            await _showtimeRepository.DeleteByDateAsync(scheduleDate);
            if (showtimesToCreate.Any())
            {
                await _showtimeRepository.CreateBatchAsync(showtimesToCreate);
            }

            // 5. 更新 DailySchedule 的 UpdatedAt
            dailySchedule.UpdatedAt = DateTime.UtcNow;
            await _dailyScheduleRepository.UpdateAsync(dailySchedule);

            // 6. 提交交易
            await transaction.CommitAsync();

            // 7. 載入完整資料並回傳
            var showtimesWithDetails = await _showtimeRepository.GetByDateWithDetailsAsync(scheduleDate);

            return new DailyScheduleResponseDto
            {
                ScheduleDate = dailySchedule.ScheduleDate,
                Status = dailySchedule.Status,
                Showtimes = showtimesWithDetails.Select(st =>
                {
                    var endTime = st.StartTime.Add(TimeSpan.FromMinutes(st.Movie.Duration));
                    return new ShowtimeResponseDto
                    {
                        Id = st.Id,
                        MovieId = st.MovieId,
                        MovieTitle = st.Movie.Title,
                        MovieDuration = st.Movie.Duration,
                        TheaterId = st.TheaterId,
                        TheaterName = st.Theater.Name,
                        TheaterType = st.Theater.Type,
                        ShowDate = st.ShowDate,
                        StartTime = st.StartTime.ToString(@"hh\:mm"),
                        EndTime = endTime.ToString(@"hh\:mm"),
                        ScheduleStatus = dailySchedule.Status,
                        CreatedAt = st.CreatedAt
                    };
                }).ToList(),
                CreatedAt = dailySchedule.CreatedAt,
                UpdatedAt = dailySchedule.UpdatedAt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 檢查場次清單內部的時間衝突
    /// </summary>
    private void CheckInternalTimeConflicts(List<MovieShowTime> showtimes, Dictionary<int, Movie> movieCache)
    {
        // 按影廳分組
        var groupedByTheater = showtimes.GroupBy(st => st.TheaterId);

        foreach (var group in groupedByTheater)
        {
            var theaterShowtimes = group.OrderBy(st => st.StartTime).ToList();

            for (int i = 0; i < theaterShowtimes.Count; i++)
            {
                var current = theaterShowtimes[i];
                var currentMovie = movieCache[current.MovieId];
                var currentEndTime = current.StartTime.Add(TimeSpan.FromMinutes(currentMovie.Duration));

                // 檢查與後續場次的衝突
                for (int j = i + 1; j < theaterShowtimes.Count; j++)
                {
                    var next = theaterShowtimes[j];
                    var nextMovie = movieCache[next.MovieId];
                    var nextEndTime = next.StartTime.Add(TimeSpan.FromMinutes(nextMovie.Duration));

                    // 檢查重疊
                    if (current.StartTime < nextEndTime && currentEndTime > next.StartTime)
                    {
                        throw new InvalidOperationException(
                            $"影廳 ID {current.TheaterId} 的場次時間衝突：{current.StartTime:hh\\:mm} 與 {next.StartTime:hh\\:mm}");
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task<DailyScheduleResponseDto> PublishDailyScheduleAsync(DateTime date)
    {
        // 正規化日期
        var scheduleDate = date.Date;

        // 1. 檢查 DailySchedule 是否存在
        var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(scheduleDate);
        if (dailySchedule == null)
        {
            throw new KeyNotFoundException($"日期 {scheduleDate:yyyy-MM-dd} 的時刻表不存在");
        }

        // 2. 檢查狀態（冪等性：已是 OnSale 則直接返回成功）
        if (dailySchedule.Status == "OnSale")
        {
            // 已是 OnSale，直接載入並返回
            var existingShowtimes = await _showtimeRepository.GetByDateWithDetailsAsync(scheduleDate);
            return BuildDailyScheduleResponse(dailySchedule, existingShowtimes);
        }

        // 3. 更新狀態為 OnSale
        dailySchedule.Status = "OnSale";
        await _dailyScheduleRepository.UpdateAsync(dailySchedule);

        // 4. 載入場次並回傳
        var showtimes = await _showtimeRepository.GetByDateWithDetailsAsync(scheduleDate);
        return BuildDailyScheduleResponse(dailySchedule, showtimes);
    }

    /// <summary>
    /// 建立 DailyScheduleResponseDto
    /// </summary>
    private DailyScheduleResponseDto BuildDailyScheduleResponse(
        DailySchedule dailySchedule,
        List<MovieShowTime> showtimes)
    {
        return new DailyScheduleResponseDto
        {
            ScheduleDate = dailySchedule.ScheduleDate,
            Status = dailySchedule.Status,
            Showtimes = showtimes.Select(st =>
            {
                var endTime = st.StartTime.Add(TimeSpan.FromMinutes(st.Movie.Duration));
                return new ShowtimeResponseDto
                {
                    Id = st.Id,
                    MovieId = st.MovieId,
                    MovieTitle = st.Movie.Title,
                    MovieDuration = st.Movie.Duration,
                    TheaterId = st.TheaterId,
                    TheaterName = st.Theater.Name,
                    TheaterType = st.Theater.Type,
                    ShowDate = st.ShowDate,
                    StartTime = st.StartTime.ToString(@"hh\:mm"),
                    EndTime = endTime.ToString(@"hh\:mm"),
                    ScheduleStatus = dailySchedule.Status,
                    CreatedAt = st.CreatedAt
                };
            }).ToList(),
            CreatedAt = dailySchedule.CreatedAt,
            UpdatedAt = dailySchedule.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<DailyScheduleResponseDto> GetDailyScheduleAsync(DateTime date)
    {
        // 正規化日期
        var scheduleDate = date.Date;

        // 1. 查詢 DailySchedule
        var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(scheduleDate);
        if (dailySchedule == null)
        {
            throw new KeyNotFoundException($"日期 {scheduleDate:yyyy-MM-dd} 的時刻表不存在");
        }

        // 2. 查詢該日期的所有場次（含電影和影廳資料）
        var showtimes = await _showtimeRepository.GetByDateWithDetailsAsync(scheduleDate);

        // 3. 建立並返回回應
        return BuildDailyScheduleResponse(dailySchedule, showtimes);
    }
}
