using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Repositories.Interfaces;
using betterthanvieshow.Services.Interfaces;

namespace betterthanvieshow.Services.Implementations;

/// <summary>
/// 票券 Service 實作
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TicketScanResponseDto> ScanTicketByQrCodeAsync(string qrCode)
    {
        // QR Code 內容即為票券編號
        var ticketNumber = qrCode;

        // 查詢票券及關聯資料
        var ticket = await _ticketRepository.GetByTicketNumberWithDetailsAsync(ticketNumber);

        if (ticket == null)
        {
            _logger.LogWarning("票券不存在: {TicketNumber}", ticketNumber);
            throw new KeyNotFoundException("票券不存在");
        }

        // 組裝回應資料
        var response = new TicketScanResponseDto
        {
            TicketId = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Status = ticket.Status,
            MovieTitle = ticket.ShowTime.Movie.Title,
            ShowDate = ticket.ShowTime.ShowDate.ToString("yyyy-MM-dd"),
            ShowTime = ticket.ShowTime.StartTime.ToString(@"hh\:mm"),
            SeatRow = ticket.Seat.RowName,
            SeatColumn = ticket.Seat.ColumnNumber,
            SeatLabel = $"{ticket.Seat.RowName} 排 {ticket.Seat.ColumnNumber} 號",
            TheaterName = ticket.ShowTime.Theater.Name,
            TheaterType = ticket.ShowTime.Theater.Type
        };

        _logger.LogInformation(
            "成功掃描票券: {TicketNumber}, 場次: {MovieTitle} ({ShowDate} {ShowTime})",
            ticketNumber,
            response.MovieTitle,
            response.ShowDate,
            response.ShowTime);

        return response;
    }
}
