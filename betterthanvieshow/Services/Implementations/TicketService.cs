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
    private readonly ITicketValidateLogRepository _validateLogRepository;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidateLogRepository validateLogRepository,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _validateLogRepository = validateLogRepository;
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

    /// <inheritdoc />
    public async Task ValidateTicketAsync(int ticketId, int validatedBy)
    {
        try
        {
            // 查詢票券
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
            {
                _logger.LogWarning("驗票失敗：票券不存在 (TicketId: {TicketId})", ticketId);
                
                // 建立失敗記錄
                await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
                {
                    TicketId = ticketId,
                    ValidatedBy = validatedBy,
                    ValidationResult = false
                });
                
                throw new KeyNotFoundException("票券不存在");
            }

            // 檢查票券狀態
            switch (ticket.Status)
            {
                case "Pending":
                    _logger.LogWarning("驗票失敗：票券未支付 (TicketNumber: {TicketNumber})", ticket.TicketNumber);
                    await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
                    {
                        TicketId = ticketId,
                        ValidatedBy = validatedBy,
                        ValidationResult = false
                    });
                    throw new InvalidOperationException("票券未支付");

                case "Used":
                    _logger.LogWarning("驗票失敗：票券已使用 (TicketNumber: {TicketNumber})", ticket.TicketNumber);
                    await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
                    {
                        TicketId = ticketId,
                        ValidatedBy = validatedBy,
                        ValidationResult = false
                    });
                    throw new InvalidOperationException("票券已使用");

                case "Expired":
                    _logger.LogWarning("驗票失敗：票券已過期 (TicketNumber: {TicketNumber})", ticket.TicketNumber);
                    await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
                    {
                        TicketId = ticketId,
                        ValidatedBy = validatedBy,
                        ValidationResult = false
                    });
                    throw new InvalidOperationException("票券已過期");

                case "Unused":
                    // 允許驗票，繼續處理
                    break;

                default:
                    _logger.LogWarning("驗票失敗：未知的票券狀態 (TicketNumber: {TicketNumber}, Status: {Status})", 
                        ticket.TicketNumber, ticket.Status);
                    await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
                    {
                        TicketId = ticketId,
                        ValidatedBy = validatedBy,
                        ValidationResult = false
                    });
                    throw new InvalidOperationException($"未知的票券狀態: {ticket.Status}");
            }

            // 更新票券狀態為 Used
            ticket.Status = "Used";
            await _ticketRepository.UpdateAsync(ticket);

            // 建立成功的驗票記錄
            await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
            {
                TicketId = ticketId,
                ValidatedBy = validatedBy,
                ValidationResult = true
            });

            _logger.LogInformation(
                "驗票成功 (TicketNumber: {TicketNumber}, ValidatedBy: {ValidatedBy})",
                ticket.TicketNumber, validatedBy);
        }
        catch (KeyNotFoundException)
        {
            // 重新拋出，讓 Controller 處理
            throw;
        }
        catch (InvalidOperationException)
        {
            // 重新拋出，讓 Controller 處理
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗票時發生未預期的錯誤 (TicketId: {TicketId})", ticketId);
            
            // 建立失敗記錄
            await _validateLogRepository.CreateAsync(new Models.Entities.TicketValidateLog
            {
                TicketId = ticketId,
                ValidatedBy = validatedBy,
                ValidationResult = false
            });
            
            throw;
        }
    }
}
