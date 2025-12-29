using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 訂單實體
/// 使用者的訂票訂單，若 5 分鐘內未付款，系統自動取消訂單並釋放預留的座位
/// </summary>
[Table("Order")]
public class Order
{
    /// <summary>
    /// 訂單 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 訂單編號，格式 #ABC-12345
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 使用者 ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// 場次 ID
    /// </summary>
    [Required]
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 第三方支付交易序號（如 LINE Pay Transaction ID），用於對帳與退款
    /// </summary>
    [MaxLength(100)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// 訂單建立時間
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 付款期限，訂單建立後 5 分鐘自動過期
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 訂單狀態：未付款、已付款、已取消
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// 訂單總金額，根據影廳類型動態計算
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 票券數量，每張訂單最多可訂 6 張票
    /// </summary>
    [Required]
    public int TicketCount { get; set; }

    // Navigation Properties

    /// <summary>
    /// 關聯的使用者
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 關聯的場次
    /// </summary>
    [ForeignKey("ShowTimeId")]
    public virtual MovieShowTime ShowTime { get; set; } = null!;

    /// <summary>
    /// 關聯的票券集合
    /// </summary>
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
