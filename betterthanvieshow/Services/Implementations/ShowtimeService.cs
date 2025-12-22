using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 場次服務實作
/// </summary>
public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IDailyScheduleRepository _dailyScheduleRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ITheaterRepository _theaterRepository;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        IDailyScheduleRepository dailyScheduleRepository,
        IMovieRepository movieRepository,
        ITheaterRepository theaterRepository)
    {
        _showtimeRepository = showtimeRepository;
        _dailyScheduleRepository = dailyScheduleRepository;
        _movieRepository = movieRepository;
        _theaterRepository = theaterRepository;
    }

    /// <inheritdoc />
    public async Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeRequestDto dto)
    {
        // 1. 驗證電影存在
        var movie = await _movieRepository.GetByIdAsync(dto.MovieId);
        if (movie == null)
        {
            throw new ArgumentException("電影不存在");
        }

        // 2. 驗證影廳存在
        var theaterExists = await _theaterRepository.ExistsAsync(dto.TheaterId);
        if (!theaterExists)
        {
            throw new ArgumentException("影廳不存在");
        }

        var theater = await _theaterRepository.GetByIdAsync(dto.TheaterId);

        // 3. 解析開始時間
        if (!TimeSpan.TryParse(dto.StartTime, out var startTime))
        {
            throw new ArgumentException("時間格式錯誤，必須為 HH:MM");
        }

        // 4. 驗證時間是 15 分鐘的倍數
        if (startTime.Minutes % 15 != 0)
        {
            throw new ArgumentException("開始時間必須是 15 分鐘的倍數（如 09:00, 09:15, 09:30, 09:45）");
        }

        // 5. 驗證放映日期在電影上映範圍內
        var showDate = dto.ShowDate.Date;
        if (showDate < movie.ReleaseDate.Date || showDate > movie.EndDate.Date)
        {
            throw new ArgumentException($"放映日期必須在電影上映期間內（{movie.ReleaseDate:yyyy-MM-dd} 至 {movie.EndDate:yyyy-MM-dd}）");
        }

        // 6. 檢查 DailySchedule 狀態
        var dailySchedule = await _dailyScheduleRepository.GetByDateAsync(showDate);
        
        if (dailySchedule != null && dailySchedule.Status == "OnSale")
        {
            throw new InvalidOperationException("該日期的時刻表已開始販售，無法新增場次");
        }

        // 7. 若日期無 DailySchedule，自動建立 Draft 狀態
        if (dailySchedule == null)
        {
            dailySchedule = await _dailyScheduleRepository.CreateAsync(new DailySchedule
            {
                ScheduleDate = showDate,
                Status = "Draft"
            });
        }

        // 8. 檢查時間衝突
        var hasConflict = await _showtimeRepository.HasTimeConflictAsync(
            dto.TheaterId, 
            showDate, 
            startTime, 
            movie.Duration);

        if (hasConflict)
        {
            throw new InvalidOperationException("該時段與現有場次時間衝突");
        }

        // 9. 建立場次
        var showtime = new MovieShowTime
        {
            MovieId = dto.MovieId,
            TheaterId = dto.TheaterId,
            ShowDate = showDate,
            StartTime = startTime
        };

        var createdShowtime = await _showtimeRepository.CreateAsync(showtime);

        // 10. 載入完整資料並回傳
        var showtimeWithDetails = await _showtimeRepository.GetByIdWithDetailsAsync(createdShowtime.Id);

        var endTime = startTime.Add(TimeSpan.FromMinutes(movie.Duration));

        return new ShowtimeResponseDto
        {
            Id = showtimeWithDetails!.Id,
            MovieId = showtimeWithDetails.MovieId,
            MovieTitle = showtimeWithDetails.Movie.Title,
            MovieDuration = showtimeWithDetails.Movie.Duration,
            TheaterId = showtimeWithDetails.TheaterId,
            TheaterName = showtimeWithDetails.Theater.Name,
            TheaterType = showtimeWithDetails.Theater.Type,
            ShowDate = showtimeWithDetails.ShowDate,
            StartTime = startTime.ToString(@"hh\:mm"),
            EndTime = endTime.ToString(@"hh\:mm"),
            ScheduleStatus = dailySchedule.Status,
            CreatedAt = showtimeWithDetails.CreatedAt
        };
    }
}
