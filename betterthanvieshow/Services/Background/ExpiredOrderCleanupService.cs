using betterthanvieshow.Data;
using betterthanvieshow.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Services.Background;

/// <summary>
/// 過期訂單自動清理背景服務
/// </summary>
public class ExpiredOrderCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredOrderCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

    public ExpiredOrderCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ExpiredOrderCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredOrderCleanupService 已啟動");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理過期訂單時發生錯誤");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("ExpiredOrderCleanupService 已停止");
    }

    private async Task CleanupExpiredOrdersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ShowtimeHub>>();

        var now = DateTime.UtcNow;

        // 找出所有過期且狀態為 Pending 的訂單
        var expiredOrders = await dbContext.Orders
            .Include(o => o.Tickets)
            .Where(o => o.Status == "Pending" && o.ExpiresAt < now)
            .ToListAsync();

        if (expiredOrders.Count == 0)
        {
            _logger.LogDebug("沒有過期訂單需要清理");
            return;
        }

        _logger.LogInformation("找到 {Count} 筆過期訂單，開始清理", expiredOrders.Count);

        foreach (var order in expiredOrders)
        {
            try
            {
                // 更新訂單狀態為 Cancelled
                order.Status = "Cancelled";

                // 收集座位 ID 用於廣播
                var seatIds = order.Tickets.Select(t => t.SeatId).ToList();
                var showtimeId = order.ShowTimeId;

                // 更新票券狀態為 Expired
                foreach (var ticket in order.Tickets)
                {
                    ticket.Status = "Expired";
                }

                await dbContext.SaveChangesAsync();

                _logger.LogInformation("訂單 {OrderNumber} 已自動取消", order.OrderNumber);

                // 廣播座位釋放通知
                try
                {
                    var roomName = $"showtime_{showtimeId}";
                    await hubContext.Clients.Group(roomName).SendAsync("SeatStatusChanged", new
                    {
                        showtimeId = showtimeId,
                        seatIds = seatIds,
                        status = "available"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "廣播座位釋放通知失敗 (訂單 {OrderNumber})", order.OrderNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消訂單 {OrderNumber} 時發生錯誤", order.OrderNumber);
            }
        }
    }
}
