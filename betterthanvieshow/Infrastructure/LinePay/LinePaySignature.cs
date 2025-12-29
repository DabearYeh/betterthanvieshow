using System.Security.Cryptography;
using System.Text;

namespace betterthanvieshow.Infrastructure.LinePay;

/// <summary>
/// LINE Pay Online API v4 HMAC 簽章生成工具
/// </summary>
public static class LinePaySignature
{
    /// <summary>
    /// 生成 LINE Pay API 請求所需的 HMAC-SHA256 簽章
    /// </summary>
    /// <param name="channelSecret">LINE Pay Channel Secret</param>
    /// <param name="requestUri">完整的請求 URI (包含 Query String)</param>
    /// <param name="requestBody">請求 Body (JSON 字串)</param>
    /// <param name="nonce">隨機生成的 UUID Nonce</param>
    /// <returns>Base64 編碼的 HMAC-SHA256 簽章</returns>
    /// <remarks>
    /// 根據 LINE Pay Online API v4 規範：
    /// Signature = Base64(HMAC-SHA256(ChannelSecret, (ChannelSecret + RequestUri + RequestBody + Nonce)))
    /// </remarks>
    public static string GenerateSignature(
        string channelSecret,
        string requestUri,
        string requestBody,
        string nonce)
    {
        if (string.IsNullOrEmpty(channelSecret))
            throw new ArgumentNullException(nameof(channelSecret));
        
        if (string.IsNullOrEmpty(requestUri))
            throw new ArgumentNullException(nameof(requestUri));
        
        if (string.IsNullOrEmpty(nonce))
            throw new ArgumentNullException(nameof(nonce));

        // 組合簽章訊息：ChannelSecret + URI + Body + Nonce
        var signatureMessage = channelSecret + requestUri + (requestBody ?? string.Empty) + nonce;
        
        // 使用 Channel Secret 作為 HMAC 金鑰
        var keyBytes = Encoding.UTF8.GetBytes(channelSecret);
        var messageBytes = Encoding.UTF8.GetBytes(signatureMessage);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);

        // 回傳 Base64 編碼結果
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// 生成隨機 UUID Nonce (v4 格式)
    /// </summary>
    /// <returns>UUID 字串 (例如: "d9c7e2a1-4f3b-4e5a-9c8d-1a2b3c4d5e6f")</returns>
    public static string GenerateNonce()
    {
        return Guid.NewGuid().ToString();
    }
}
