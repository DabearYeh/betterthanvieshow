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
}
