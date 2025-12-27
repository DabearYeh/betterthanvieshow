using Microsoft.AspNetCore.SignalR;

namespace betterthanvieshow.Hubs;

/// <summary>
/// 場次座位即時同步 Hub
/// </summary>
public class ShowtimeHub : Hub
{
    private readonly ILogger<ShowtimeHub> _logger;

    public ShowtimeHub(ILogger<ShowtimeHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 加入場次房間
    /// </summary>
    /// <param name="showtimeId">場次 ID</param>
    public async Task JoinShowtime(int showtimeId)
    {
        var roomName = $"showtime_{showtimeId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        _logger.LogInformation("Client {ConnectionId} joined showtime {ShowtimeId}", 
            Context.ConnectionId, showtimeId);
    }

    /// <summary>
    /// 離開場次房間
    /// </summary>
    /// <param name="showtimeId">場次 ID</param>
    public async Task LeaveShowtime(int showtimeId)
    {
        var roomName = $"showtime_{showtimeId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        _logger.LogInformation("Client {ConnectionId} left showtime {ShowtimeId}", 
            Context.ConnectionId, showtimeId);
    }
}
