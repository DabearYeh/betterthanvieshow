using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 票券實體
/// 使用者購買後產生的票券，包含場次、座位資訊
/// </summary>
[Table("Ticket")]
public class Ticket
{
    /// <summary>
    /// 票券 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 票券編號，唯一識別碼
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// 訂單 ID
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// 場次 ID
    /// </summary>
    [Required]
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 座位 ID
    /// </summary>
    [Required]
    public int SeatId { get; set; }

    /// <summary>
    /// QR Code，包含電影日期、影廳、座位、時間、影廳類型
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// 票券狀態：待支付、未使用、已使用、已過期
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "待支付";

    /// <summary>
    /// 票價，根據該場次所屬影廳類型決定
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    // Navigation Properties

    /// <summary>
    /// 關聯的訂單
    /// </summary>
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// 關聯的場次
    /// </summary>
    [ForeignKey("ShowTimeId")]
    public virtual MovieShowTime ShowTime { get; set; } = null!;

    /// <summary>
    /// 關聯的座位
    /// </summary>
    [ForeignKey("SeatId")]
    public virtual Seat Seat { get; set; } = null!;
}
