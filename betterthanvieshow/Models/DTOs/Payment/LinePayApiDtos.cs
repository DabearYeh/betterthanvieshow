using System.Text.Json.Serialization;

namespace betterthanvieshow.Models.DTOs.Payment;

/// <summary>
/// LINE Pay API 回應的通用格式 (v4)
/// </summary>
/// <typeparam name="T">Info 欄位的類型</typeparam>
public class LinePayApiResponse<T>
{
    /// <summary>
    /// 結果代碼 ("0000" 表示成功)
    /// </summary>
    [JsonPropertyName("returnCode")]
    public string ReturnCode { get; set; } = string.Empty;

    /// <summary>
    /// 結果訊息
    /// </summary>
    [JsonPropertyName("returnMessage")]
    public string ReturnMessage { get; set; } = string.Empty;

    /// <summary>
    /// API 結果資料 (僅成功時回傳)
    /// </summary>
    [JsonPropertyName("info")]
    public T? Info { get; set; }

    /// <summary>
    /// 是否成功 (returnCode == "0000")
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => ReturnCode == "0000";
}

/// <summary>
/// LINE Pay Request API 的 Info 結構
/// </summary>
public class LinePayRequestInfo
{
    /// <summary>
    /// LINE Pay 交易 ID (19 位數字)
    /// </summary>
    [JsonPropertyName("transactionId")]
    public long TransactionId { get; set; }

    /// <summary>
    /// 付款頁面網址
    /// </summary>
    [JsonPropertyName("paymentUrl")]
    public LinePayPaymentUrl PaymentUrl { get; set; } = new();
}

/// <summary>
/// LINE Pay 付款頁面網址
/// </summary>
public class LinePayPaymentUrl
{
    /// <summary>
    /// 網頁版付款網址
    /// </summary>
    [JsonPropertyName("web")]
    public string Web { get; set; } = string.Empty;

    /// <summary>
    /// App 版付款網址
    /// </summary>
    [JsonPropertyName("app")]
    public string App { get; set; } = string.Empty;
}

/// <summary>
/// LINE Pay Confirm API 的 Info 結構
/// </summary>
public class LinePayConfirmInfo
{
    /// <summary>
    /// 商店訂單編號
    /// </summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// LINE Pay 交易 ID
    /// </summary>
    [JsonPropertyName("transactionId")]
    public long TransactionId { get; set; }

    /// <summary>
    /// 付款資訊列表 (可能混合多種支付方式)
    /// </summary>
    [JsonPropertyName("payInfo")]
    public List<LinePayPayInfo> PayInfo { get; set; } = new();
}

/// <summary>
/// 付款方式資訊
/// </summary>
public class LinePayPayInfo
{
    /// <summary>
    /// 付款方式 (例如：BALANCE, CREDIT_CARD, POINT)
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 該方式支付的金額
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

/// <summary>
/// LINE Pay Request API 的請求 Body
/// </summary>
public class LinePayRequestBody
{
    /// <summary>
    /// 付款金額（整數，單位：元）
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 幣別 (TWD)
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 商店訂單編號 (用於 LINE Pay 識別，建議使用 OrderNumber)
    /// </summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 商品包裝列表
    /// </summary>
    [JsonPropertyName("packages")]
    public List<LinePayPackage> Packages { get; set; } = new();

    /// <summary>
    /// 跳轉網址設定
    /// </summary>
    [JsonPropertyName("redirectUrls")]
    public LinePayRedirectUrls RedirectUrls { get; set; } = new();
}

/// <summary>
/// 商品包裝 (LINE Pay 要求至少有一個 Package)
/// </summary>
public class LinePayPackage
{
    /// <summary>
    /// Package ID (通常設為 "1")
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "1";

    /// <summary>
    /// Package 總金額（整數，單位：元）
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 商品列表
    /// </summary>
    [JsonPropertyName("products")]
    public List<LinePayProduct> Products { get; set; } = new();
}

/// <summary>
/// 商品資訊
/// </summary>
public class LinePayProduct
{
    /// <summary>
    /// 商品名稱
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 數量
    /// </summary>
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 單價（整數，單位：元）
    /// </summary>
    [JsonPropertyName("price")]
    public int Price { get; set; }
}

/// <summary>
/// 跳轉網址設定
/// </summary>
public class LinePayRedirectUrls
{
    /// <summary>
    /// 付款成功後跳轉的網址
    /// </summary>
    [JsonPropertyName("confirmUrl")]
    public string ConfirmUrl { get; set; } = string.Empty;

    /// <summary>
    /// 付款取消後跳轉的網址
    /// </summary>
    [JsonPropertyName("cancelUrl")]
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// LINE Pay Confirm API 的請求 Body
/// </summary>
public class LinePayConfirmBody
{
    /// <summary>
    /// 付款金額（整數，必須與 Request 時相同）
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "TWD";
}
