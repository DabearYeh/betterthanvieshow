using betterthanvieshow.Models.DTOs;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 票券 Service 介面
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// 掃描票券 QR Code 並取得票券詳細資訊
    /// </summary>
    /// <param name="qrCode">QR Code 內容（票券編號）</param>
    /// <returns>票券詳細資訊</returns>
    Task<TicketScanResponseDto> ScanTicketByQrCodeAsync(string qrCode);

    /// <summary>
    /// 執行驗票
    /// </summary>
    /// <param name="ticketId">票券 ID</param>
    /// <param name="validatedBy">驗票人員 ID（管理者）</param>
    Task ValidateTicketAsync(int ticketId, int validatedBy);
}
