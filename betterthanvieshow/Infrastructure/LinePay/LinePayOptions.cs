namespace betterthanvieshow.Infrastructure.LinePay;

/// <summary>
/// LINE Pay API 設定選項
/// </summary>
public class LinePayOptions
{
    /// <summary>
    /// LINE Pay Channel ID
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// LINE Pay Channel Secret (用於 HMAC 簽章)
    /// </summary>
    public string ChannelSecret { get; set; } = string.Empty;

    /// <summary>
    /// 是否使用 Sandbox 環境
    /// </summary>
    public bool IsSandbox { get; set; } = true;

    /// <summary>
    /// LINE Pay API 基礎網址
    /// </summary>
    /// <example>
    /// Sandbox: https://sandbox-api-pay.line.me
    /// Production: https://api-pay.line.me
    /// </example>
    public string ApiBaseUrl { get; set; } = "https://sandbox-api-pay.line.me";

    /// <summary>
    /// 前端網站基礎網址
    /// </summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 付款確認頁面路徑 (相對於 FrontendBaseUrl)
    /// </summary>
    public string ConfirmUrlPath { get; set; } = "/checkout/confirm";

    /// <summary>
    /// 付款取消頁面路徑 (相對於 FrontendBaseUrl)
    /// </summary>
    public string CancelUrlPath { get; set; } = "/checkout/cancel";

    /// <summary>
    /// 取得完整的 Confirm URL
    /// </summary>
    public string GetConfirmUrl() => $"{FrontendBaseUrl.TrimEnd('/')}{ConfirmUrlPath}";

    /// <summary>
    /// 取得完整的 Cancel URL
    /// </summary>
    public string GetCancelUrl() => $"{FrontendBaseUrl.TrimEnd('/')}{CancelUrlPath}";
}
