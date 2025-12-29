using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace betterthanvieshow.Infrastructure.LinePay;

/// <summary>
/// LINE Pay API HTTP Client 封裝
/// </summary>
public class LinePayHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly LinePayOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public LinePayHttpClient(
        HttpClient httpClient,
        IOptions<LinePayOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        
        // 設定 JSON 序列化選項 (camelCase)
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// 發送 POST 請求至 LINE Pay API
    /// </summary>
    /// <typeparam name="TResponse">回應 DTO 類型</typeparam>
    /// <param name="apiPath">API 路徑 (例如: "/v4/payments/request")</param>
    /// <param name="requestBody">請求 Body 物件</param>
    /// <returns>API 回應物件</returns>
    public async Task<TResponse> PostAsync<TResponse>(string apiPath, object requestBody)
    {
        var nonce = LinePaySignature.GenerateNonce();
        var requestUri = apiPath;
        var requestBodyJson = JsonSerializer.Serialize(requestBody, _jsonOptions);

        // 生成 HMAC 簽章
        var signature = LinePaySignature.GenerateSignature(
            _options.ChannelSecret,
            requestUri,
            requestBodyJson,
            nonce
        );

        // 設定 HTTP 請求
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json")
        };

        // 加入 LINE Pay 必要標頭
        request.Headers.Add("X-LINE-ChannelId", _options.ChannelId);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);
        request.Headers.Add("X-LINE-Authorization", signature);

        // 發送請求
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        // 反序列化回應 (即使 HTTP 狀態非 200 也要解析，因為 LINE Pay 總是回傳 200)
        var result = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);

        if (result == null)
        {
            throw new InvalidOperationException($"無法解析 LINE Pay API 回應: {responseContent}");
        }

        return result;
    }

    /// <summary>
    /// 發送 GET 請求至 LINE Pay API
    /// </summary>
    /// <typeparam name="TResponse">回應 DTO 類型</typeparam>
    /// <param name="apiPath">API 路徑 (例如: "/v4/payments")</param>
    /// <param name="queryString">查詢字串 (例如: "?orderId=123")</param>
    /// <returns>API 回應物件</returns>
    public async Task<TResponse> GetAsync<TResponse>(string apiPath, string queryString = "")
    {
        var nonce = LinePaySignature.GenerateNonce();
        var requestUri = apiPath + queryString;

        // GET 請求沒有 Body，傳空字串
        var signature = LinePaySignature.GenerateSignature(
            _options.ChannelSecret,
            requestUri,
            string.Empty,
            nonce
        );

        // 設定 HTTP 請求
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // 加入 LINE Pay 必要標頭
        request.Headers.Add("X-LINE-ChannelId", _options.ChannelId);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);
        request.Headers.Add("X-LINE-Authorization", signature);

        // 發送請求
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        // 反序列化回應
        var result = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);

        if (result == null)
        {
            throw new InvalidOperationException($"無法解析 LINE Pay API 回應: {responseContent}");
        }

        return result;
    }
}
