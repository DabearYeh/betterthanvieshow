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

    /// <inheritdoc />
    public async Task<MonthOverviewResponseDto> GetMonthOverviewAsync(int year, int month)
    {
        // 從 Repository 獲取該月份的所有時刻表
        var schedules = await _dailyScheduleRepository.GetByMonthAsync(year, month);

        // 轉換為 DTO
        var dates = schedules.Select(s => new DailyScheduleStatusDto
        {
            Date = s.ScheduleDate.ToString("yyyy-MM-dd"),
            Status = s.Status
        }).ToList();

        return new MonthOverviewResponseDto
        {
            Year = year,
            Month = month,
            Dates = dates
        };
    }

    /// <inheritdoc />
    public async Task<CopyDailyScheduleResponseDto> CopyDailyScheduleAsync(DateTime sourceDate, CopyDailyScheduleRequestDto dto)
    {
        // 正規化日期
        var sourceDateNormalized = sourceDate.Date;

        // 解析目標日期
        if (!DateTime.TryParse(dto.TargetDate, out var targetDate))
        {
            throw new ArgumentException("目標日期格式錯誤，必須為 YYYY-MM-DD");
        }
        var targetDateNormalized = targetDate.Date;

        // 開啟交易
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. 驗證來源時刻表
            var sourceSchedule = await _dailyScheduleRepository.GetByDateAsync(sourceDateNormalized);
            if (sourceSchedule == null)
            {
                throw new KeyNotFoundException($"來源日期 {sourceDateNormalized:yyyy-MM-dd} 的時刻表不存在");
            }

            if (sourceSchedule.Status != "OnSale")
            {
                throw new ArgumentException("只能複製已販售的時刻表");
            }

            // 2. 驗證/建立目標時刻表
            var targetSchedule = await _dailyScheduleRepository.GetByDateAsync(targetDateNormalized);

            if (targetSchedule != null && targetSchedule.Status == "OnSale")
            {
                throw new ArgumentException("目標日期必須為草稿狀態");
            }

            if (targetSchedule == null)
            {
                // 建立新的 Draft 狀態時刻表
                targetSchedule = await _dailyScheduleRepository.CreateAsync(new DailySchedule
                {
                    ScheduleDate = targetDateNormalized,
                    Status = "Draft"
                });
            }

            // 3. 刪除目標日期的舊場次（覆蓋模式）
            await _showtimeRepository.DeleteByDateAsync(targetDateNormalized);

            // 4. 取得來源場次（包含電影資訊）
            var sourceShowtimes = await _showtimeRepository.GetByDateWithMovieAsync(sourceDateNormalized);

            // 5. 檔期檢查並複製
            var showtimesToCopy = new List<MovieShowTime>();
            var skippedCount = 0;

            foreach (var sourceShowtime in sourceShowtimes)
            {
                // 檢查電影在目標日期是否仍在檔期內
                if (sourceShowtime.Movie.ReleaseDate.Date <= targetDateNormalized &&
                    targetDateNormalized <= sourceShowtime.Movie.EndDate.Date)
                {
                    // 在檔期內，加入待複製清單
                    showtimesToCopy.Add(new MovieShowTime
                    {
                        MovieId = sourceShowtime.MovieId,
                        TheaterId = sourceShowtime.TheaterId,
                        ShowDate = targetDateNormalized,
                        StartTime = sourceShowtime.StartTime
                    });
                }
                else
                {
                    // 檔期已過，略過
                    skippedCount++;
                }
            }

            // 6. 批次建立新場次
            if (showtimesToCopy.Any())
            {
                await _showtimeRepository.CreateBatchAsync(showtimesToCopy);
            }

            // 7. 更新目標時刻表的 UpdatedAt
            targetSchedule.UpdatedAt = DateTime.UtcNow;
            await _dailyScheduleRepository.UpdateAsync(targetSchedule);

            // 8. 提交交易
            await transaction.CommitAsync();

            // 9. 載入完整的目標時刻表資料
            var targetShowtimesWithDetails = await _showtimeRepository.GetByDateWithDetailsAsync(targetDateNormalized);
            var targetScheduleResponse = BuildDailyScheduleResponse(targetSchedule, targetShowtimesWithDetails);

            // 10. 建立回應
            var response = new CopyDailyScheduleResponseDto
            {
                SourceDate = sourceDateNormalized,
                TargetDate = targetDateNormalized,
                CopiedCount = showtimesToCopy.Count,
                SkippedCount = skippedCount,
                TargetSchedule = targetScheduleResponse
            };

            // 如有場次被略過，加入提示訊息
            if (skippedCount > 0)
            {
                response.Message = "部分場次因電影檔期已過期未複製";
            }

            return response;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<GroupedDailyScheduleResponseDto> GetGroupedDailyScheduleAsync(DateTime date)
    {
        var scheduleDate = date.Date;

        // 1. 查詢時刻表
        var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(scheduleDate);
        if (dailySchedule == null)
        {
            throw new KeyNotFoundException($"日期 {scheduleDate:yyyy-MM-dd} 的時刻表不存在");
        }

        // 2. 取得該日期的所有場次（包含電影、影廳資訊）
        var showtimes = await _showtimeRepository.GetByDateWithDetailsAsync(scheduleDate);

        // 3. 按電影分組
        var movieGroups = showtimes
            .GroupBy(s => new { s.MovieId, s.Movie.Title, s.Movie.PosterUrl, s.Movie.Rating, s.Movie.Duration })
            .Select(movieGroup =>
            {
                // 4. 每個電影內，按影廳類型分組
                var theaterTypeGroups = movieGroup
                    .GroupBy(s => s.Theater.Type)
                    .Select(typeGroup =>
                    {
                        var showtimesList = typeGroup
                            .Select(s => new ShowtimeSimpleDto
                            {
                                Id = s.Id,
                                TheaterId = s.TheaterId,
                                TheaterName = s.Theater.Name,
                                StartTime = s.StartTime.ToString(@"hh\:mm"),
                                EndTime = s.StartTime.Add(TimeSpan.FromMinutes(s.Movie.Duration)).ToString(@"hh\:mm")
                            })
                            .OrderBy(s => s.StartTime)
                            .ToList();

                        // 計算時間範圍
                        var minStartTime = showtimesList.Min(s => s.StartTime);
                        var maxEndTime = showtimesList.Max(s => s.EndTime);
                        var timeRange = minStartTime == maxEndTime ? minStartTime : $"{minStartTime} {maxEndTime}";

                        return new TheaterTypeGroupDto
                        {
                            TheaterType = typeGroup.Key ?? string.Empty,
                            TheaterTypeDisplay = ConvertTheaterTypeToDisplay(typeGroup.Key ?? string.Empty),
                            TimeRange = timeRange,
                            Showtimes = showtimesList
                        };
                    })
                    .OrderBy(t => t.TheaterType)
                    .ToList();

                return new MovieShowtimeGroupDto
                {
                    MovieId = movieGroup.Key.MovieId,
                    MovieTitle = movieGroup.Key.Title,
                    PosterUrl = movieGroup.Key.PosterUrl,
                    Rating = movieGroup.Key.Rating,
                    RatingDisplay = ConvertRatingToDisplay(movieGroup.Key.Rating),
                    Duration = movieGroup.Key.Duration,
                    DurationDisplay = FormatDuration(movieGroup.Key.Duration),
                    TheaterTypeGroups = theaterTypeGroups
                };
            })
            .OrderBy(m => m.TheaterTypeGroups.Min(t => t.Showtimes.Min(s => s.StartTime)))
            .ToList();

        return new GroupedDailyScheduleResponseDto
        {
            ScheduleDate = scheduleDate,
            Status = dailySchedule.Status,
            MovieShowtimes = movieGroups
        };
    }

    /// <summary>
    /// 轉換電影分級為顯示格式
    /// </summary>
    private string ConvertRatingToDisplay(string rating)
    {
        return rating switch
        {
            "G" => "0+",      // General Audiences
            "PG" => "12+",    // Parental Guidance
            "R" => "18+",     // Restricted
            _ => "0+"
        };
    }

    /// <summary>
    /// 格式化片長顯示
    /// </summary>
    private string FormatDuration(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return $"{hours} 小時 {mins} 分鐘";
    }

    /// <summary>
    /// 轉換影廳類型為顯示格式
    /// </summary>
    private string ConvertTheaterTypeToDisplay(string theaterType)
    {
        return theaterType switch
        {
            "Digital" => "數位",
            "4DX" => "4DX",
            "IMAX" => "IMAX",
            _ => theaterType
        };
    }

}
