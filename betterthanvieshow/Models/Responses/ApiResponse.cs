namespace betterthanvieshow.Models.Responses;

/// <summary>
/// API 標準回應格式
/// </summary>
/// <typeparam name="T">回應資料類型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 回應訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 回應資料
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 錯誤詳情（驗證錯誤等）
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// 建立成功回應
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 建立失敗回應
    /// </summary>
    public static ApiResponse<T> FailureResponse(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
