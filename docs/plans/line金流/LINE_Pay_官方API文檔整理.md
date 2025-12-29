# LINE Pay Online API v3 官方文檔整理

## 📚 文檔來源
LINE Pay 官方開發者文檔 - Online API v3

---

## 🔐 1. 認證機制 (Credentials)

### 取得憑證
1. **正式環境**：在合作商店中心 → [管理付款連結] → [管理連結金鑰] 取得
2. **測試環境**：申請 Sandbox 帳號取得

### 必要憑證
- **Channel ID**：通訊管道識別碼
- **Channel Secret**：用於生成 HMAC 簽章的金鑰

### HTTP 標頭配置

每次呼叫 API 都必須在 HTTP 標頭中包含以下三個欄位：

| 標頭名稱 | 說明 | 範例 |
|---------|------|------|
| `X-LINE-ChannelId` | Channel ID | `1234567890` |
| `X-LINE-Authorization` | HMAC-SHA256 簽章（Base64 編碼） | `hmacBase64String` |
| `X-LINE-Authorization-Nonce` | UUID v1/v4 或時間戳記 | `550e8400-e29b-41d4-a716-446655440000` |

### HMAC 簽章生成規則

簽章訊息 (signature) 根據 HTTP 方法不同而有所差異：

#### GET 方法
```
MAC生成訊息 = Channel Secret + API路徑 + 查詢字串 + Nonce
```

**範例：**
```
channelSecret = "abc123secret"
apiPath = "/v3/payments/1234567890"
queryString = ""
nonce = "550e8400-e29b-41d4-a716-446655440000"

訊息 = "abc123secret/v3/payments/1234567890550e8400-e29b-41d4-a716-446655440000"
```

#### POST 方法
```
MAC生成訊息 = Channel Secret + API路徑 + Request Body（JSON字串） + Nonce
```

**範例：**
```
channelSecret = "abc123secret"
apiPath = "/v3/payments/request"
requestBody = '{"amount":100,"currency":"TWD",...}'
nonce = "550e8400-e29b-41d4-a716-446655440000"

訊息 = "abc123secret/v3/payments/request{\"amount\":100,\"currency\":\"TWD\",...}550e8400-e29b-41d4-a716-446655440000"
```

### 簽章生成步驟

1. **組合訊息**：按照上述規則組合訊息字串
2. **HMAC-SHA256 加密**：使用 Channel Secret 作為金鑰
3. **Base64 編碼**：將加密結果轉為 Base64 字串
4. **放入標頭**：將結果填入 `X-LINE-Authorization`

---

## 🔄 2. 完整付款流程

### 流程概覽

```
1. 付款請求 (Request API)
   ↓
2. LINE Pay 認證 (User Authorization)
   ↓
3. 付款授權 (Confirm API)
   ↓
4. 完成付款
```

---

## 📤 3. 付款請求 API (Request API)

### 基本資訊

| 項目 | 內容 |
|------|------|
| **HTTP 方法** | `POST` |
| **Sandbox URL** | `https://sandbox-api-pay.line.me/v3/payments/request` |
| **Production URL** | `https://api-pay.line.me/v3/payments/request` |

### 請求範例

```json
{
  "amount": 100,
  "currency": "TWD",
  "orderId": "EXAMPLE_ORDER_20230422_1000001",
  "packages": [
    {
      "id": "1",
      "amount": 100,
      "products": [
        {
          "id": "PEN-B-001",
          "name": "Pen Brown",
          "imageUrl": "https://store.example.com/images/pen_brown.jpg",
          "quantity": 2,
          "price": 50
        }
      ]
    }
  ],
  "redirectUrls": {
    "confirmUrl": "https://store.example.com/order/payment/authorize",
    "cancelUrl": "https://store.example.com/order/payment/cancel"
  }
}
```

### 請求欄位說明

#### 主要欄位

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| `amount` | Number | ✅ | 付款總金額 |
| `currency` | String | ✅ | 貨幣代碼（台灣使用 `TWD`） |
| `orderId` | String | ✅ | 合作商店的訂單編號（在您的專案中對應 `Order.OrderNumber`） |
| `packages` | Array | ✅ | 商品包裹資訊（至少一個） |
| `redirectUrls` | Object | ✅ | 重定向 URL 設定 |

#### Packages 欄位

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| `id` | String | ✅ | 包裹 ID（通常為 "1"） |
| `amount` | Number | ✅ | 包裹金額（必須等於所有 products 的總和） |
| `products` | Array | ✅ | 商品列表 |

#### Products 欄位

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| `id` | String | ⭕ | 商品 ID |
| `name` | String | ✅ | 商品名稱（在您的專案中可顯示為「電影票券」） |
| `imageUrl` | String | ⭕ | 商品圖片 URL（可用電影海報） |
| `quantity` | Number | ✅ | 商品數量（在您的專案中對應票券數量） |
| `price` | Number | ✅ | 單價 |

#### RedirectUrls 欄位

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| `confirmUrl` | String | ✅ | 付款成功後的重定向 URL（顧客完成 LINE Pay 認證後跳轉） |
| `cancelUrl` | String | ✅ | 付款取消後的重定向 URL（顧客取消付款時跳轉） |

### 回應範例（成功）

```json
{
  "returnCode": "0000",
  "returnMessage": "Success.",
  "info": {
    "paymentUrl": {
      "web": "https://sandbox-web-pay.line.me/web/payment/wait?transactionReserveId=REpEWEttQ0F2RmFnaFFzVndIdjl6Z0lqbGpPemZjOHpNWTFZTmdibUlRNlEzOG50N2VSRmdGU2IxcnVjMHZ1NQ",
      "app": "line://pay/payment/REpEWEttQ0F2RmFnaFFzVndIdjl6Z0lqbGpPemZjOHpNWTFZTmdibUlRNlEzOG50N2VSRmdGU2IxcnVjMHZ1NQ"
    },
    "transactionId": 2023042201206549310,
    "paymentAccessToken": "056579816895"
  }
}
```

### 回應欄位說明

| 欄位 | 類型 | 說明 |
|------|------|------|
| `returnCode` | String | 結果代碼（`0000` = 成功） |
| `returnMessage` | String | 結果訊息 |
| `info.paymentUrl.web` | String | PC 版付款頁面 URL |
| `info.paymentUrl.app` | String | APP 版付款深層連結 |
| `info.transactionId` | Number | **交易 ID（重要！用於後續 Confirm API）** |
| `info.paymentAccessToken` | String | 付款存取權杖 |

### ⚠️ 重要提醒：交易 ID 處理

**問題：** JavaScript/Node.js 的 `Number` 類型無法安全處理超過 `2^53 - 1` 的整數，而 LINE Pay 的 `transactionId` 可能超過此範圍。

**解決方案：**
1. 使用 `BigInt` 類型處理
2. 將 `transactionId` 視為字串儲存
3. 使用 `handleBigInteger()` 函數處理 JSON 回應

**在 C# 中不需要擔心此問題**，因為 C# 的 `long` 類型可以安全處理 64-bit 整數。

### 專案對應範例

根據您的專案規格，電影院訂票的 Request API 應該這樣組裝：

```json
{
  "amount": 1140,
  "currency": "TWD",
  "orderId": "#ABC-12345",
  "packages": [
    {
      "id": "1",
      "amount": 1140,
      "products": [
        {
          "name": "電影票券",
          "imageUrl": "https://yourdomain.com/api/movies/poster/123",
          "quantity": 3,
          "price": 380
        }
      ]
    }
  ],
  "redirectUrls": {
    "confirmUrl": "https://yourdomain.com/payments/confirm?orderId=123",
    "cancelUrl": "https://yourdomain.com/payments/cancel?orderId=123"
  }
}
```

**欄位對應：**
- `amount` → `Order.TotalPrice`（訂單總金額）
- `orderId` → `Order.OrderNumber`（訂單編號，如 `#ABC-12345`）
- `products[0].quantity` → `Order.TicketCount`（票券數量）
- `products[0].price` → `Ticket.Price`（單張票價，根據影廳類型）

---

## 🔐 4. LINE Pay 認證流程

### 使用者認證步驟

1. **跳轉至付款頁面**
   - PC 使用者：開啟 `info.paymentUrl.web`（建議在彈出視窗）
   - 行動裝置：使用 `info.paymentUrl.app` 深層連結啟動 LINE APP

2. **使用者操作**
   - 在 LINE Pay 頁面登入
   - 選擇付款方式（LINE Points 或綁定的信用卡）
   - 輸入付款密碼

3. **使用者決定**
   - **完成認證** → 跳轉至 `confirmUrl`
   - **取消付款** → 跳轉至 `cancelUrl`

### 前端實作建議

#### PC 版（彈出視窗）
```javascript
// 開啟付款視窗
const paymentWindow = window.open(
  response.info.paymentUrl.web,
  'LINE Pay',
  'width=600,height=800'
);

// 監聽視窗關閉
const checkClosed = setInterval(() => {
  if (paymentWindow.closed) {
    clearInterval(checkClosed);
    // 檢查付款狀態
  }
}, 1000);
```

#### 行動版（深層連結）
```javascript
// 嘗試開啟 LINE APP
window.location.href = response.info.paymentUrl.app;

// 備用方案：如果無法開啟 APP，使用 web 版
setTimeout(() => {
  window.location.href = response.info.paymentUrl.web;
}, 1500);
```

---

## ✅ 5. 付款授權 API (Confirm API)

### 基本資訊

| 項目 | 內容 |
|------|------|
| **HTTP 方法** | `POST` |
| **Sandbox URL** | `https://sandbox-api-pay.line.me/v3/payments/{transactionId}/confirm` |
| **Production URL** | `https://api-pay.line.me/v3/payments/{transactionId}/confirm` |

### 請求範例

```http
POST /v3/payments/2023042201206549310/confirm
Content-Type: application/json
X-LINE-ChannelId: 1234567890
X-LINE-Authorization: {signature}
X-LINE-Authorization-Nonce: {nonce}

{
  "amount": 100,
  "currency": "TWD"
}
```

### 請求欄位說明

| 欄位 | 類型 | 必填 | 說明 |
|------|------|------|------|
| `transactionId` | Number/String | ✅ | **URL 路徑中的交易 ID**（來自 Request API 回應） |
| `amount` | Number | ✅ | 付款金額（必須與 Request API 時的金額一致） |
| `currency` | String | ✅ | 貨幣代碼（必須與 Request API 時的貨幣一致） |

### ⚠️ 關鍵規則

1. **金額驗證**：Confirm API 的 `amount` 和 `currency` 必須與 Request API 完全一致
2. **自動請款**：預設情況下，Confirm API 完成後會**自動請款**，無法取消授權
3. **不可重複確認**：同一個 `transactionId` 只能成功 Confirm 一次

### 回應範例（成功）

```json
{
  "returnCode": "0000",
  "returnMessage": "OK",
  "info": {
    "orderId": "EXAMPLE_ORDER_20230422_1000001",
    "transactionId": 2023042201206549310,
    "payInfo": [
      {
        "method": "BALANCE",
        "amount": 100
      }
    ]
  }
}
```

### 回應欄位說明

| 欄位 | 類型 | 說明 |
|------|------|------|
| `returnCode` | String | 結果代碼（`0000` = 成功） |
| `returnMessage` | String | 結果訊息 |
| `info.orderId` | String | 合作商店訂單編號（您傳入的 `Order.OrderNumber`） |
| `info.transactionId` | Number | LINE Pay 交易 ID |
| `info.payInfo` | Array | 付款方式詳情 |
| `info.payInfo[].method` | String | 付款方式（`BALANCE` = LINE Points，`CARD` = 信用卡） |
| `info.payInfo[].amount` | Number | 該付款方式的金額 |

### 專案整合後需執行的動作

當 `returnCode` 為 `0000` 時，表示付款成功，您需要：

1. **更新訂單狀態**
   ```csharp
   order.Status = "Paid";
   order.PaymentTransactionId = response.info.transactionId.ToString();
   ```

2. **更新票券狀態**
   ```csharp
   foreach (var ticket in order.Tickets)
   {
       ticket.Status = "Unused";
   }
   ```

3. **生成 QR Code**
   ```csharp
   foreach (var ticket in order.Tickets)
   {
       ticket.QrCode = GenerateQrCode(ticket);
   }
   ```

4. **儲存變更**
   ```csharp
   await _dbContext.SaveChangesAsync();
   ```

5. **回傳成功訊息給前端**

---

## 🛠️ 6. C# 實作參考

### HMAC 簽章生成範例

```csharp
using System.Security.Cryptography;
using System.Text;

public class LinePaySignature
{
    public static string GenerateSignature(string channelSecret, string message)
    {
        var encoding = new UTF8Encoding();
        var keyBytes = encoding.GetBytes(channelSecret);
        var messageBytes = encoding.GetBytes(message);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    public static string GenerateNonce()
    {
        return Guid.NewGuid().ToString();
    }
}
```

### HTTP Client 封裝範例

```csharp
public class LinePayHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _channelId;
    private readonly string _channelSecret;

    public LinePayHttpClient(HttpClient httpClient, string channelId, string channelSecret)
    {
        _httpClient = httpClient;
        _channelId = channelId;
        _channelSecret = channelSecret;
    }

    public async Task<T> PostAsync<T>(string apiPath, object requestBody)
    {
        var nonce = LinePaySignature.GenerateNonce();
        var requestBodyJson = JsonSerializer.Serialize(requestBody);
        
        // 生成簽章訊息：channelSecret + apiPath + requestBody + nonce
        var message = _channelSecret + apiPath + requestBodyJson + nonce;
        var signature = LinePaySignature.GenerateSignature(_channelSecret, message);

        // 設定 HTTP 標頭
        var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
        request.Headers.Add("X-LINE-ChannelId", _channelId);
        request.Headers.Add("X-LINE-Authorization", signature);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);
        request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

        // 發送請求
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<T>(responseContent);
    }

    public async Task<T> GetAsync<T>(string apiPath, string queryString = "")
    {
        var nonce = LinePaySignature.GenerateNonce();
        
        // 生成簽章訊息：channelSecret + apiPath + queryString + nonce
        var message = _channelSecret + apiPath + queryString + nonce;
        var signature = LinePaySignature.GenerateSignature(_channelSecret, message);

        // 設定 HTTP 標頭
        var request = new HttpRequestMessage(HttpMethod.Get, apiPath + queryString);
        request.Headers.Add("X-LINE-ChannelId", _channelId);
        request.Headers.Add("X-LINE-Authorization", signature);
        request.Headers.Add("X-LINE-Authorization-Nonce", nonce);

        // 發送請求
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<T>(responseContent);
    }
}
```

---

## 📊 7. 結果代碼參考

### 常見成功代碼

| returnCode | 說明 |
|------------|------|
| `0000` | 成功 |

### 常見錯誤代碼

| returnCode | 說明 | 處理方式 |
|------------|------|----------|
| `1104` | 合作商店不存在 | 檢查 Channel ID |
| `1105` | 合作商店帳號未通過審核 | 等待審核通過 |
| `1106` | 標頭訊息錯誤 | 檢查 HTTP 標頭設定 |
| `1124` | 金額錯誤（超過限額） | 調整金額 |
| `1198` | API 呼叫錯誤 | 檢查請求格式 |
| `2101` | 參數錯誤 | 檢查請求參數 |
| `2102` | JSON 資料格式錯誤 | 檢查 JSON 格式 |

**完整錯誤代碼清單：** 請參考 LINE Pay 官方文檔的「回應代碼」章節

---

## 🔄 8. 完整流程時序圖（含技術細節）

```
前端                後端API                LINE Pay              資料庫
 |                    |                      |                     |
 |-- 確認訂單 -------->|                      |                     |
 |                    |-- 建立 Order ------->|                     |
 |                    |   Status: Pending    |                     |
 |                    |                      |                     |
 |                    |-- POST /v3/payments/request ---------->|   |
 |                    |   Body: {amount, orderId, ...}        |   |
 |                    |   Headers: {ChannelId, Signature, Nonce}  |
 |                    |                      |                     |
 |                    |<-- 回應 transactionId, paymentUrl -----|   |
 |                    |                      |                     |
 |<-- 回傳 paymentUrl -|                      |                     |
 |                    |                      |                     |
 |-- 跳轉 paymentUrl ->|------- LINE Pay 認證頁面 ------------>|   |
 |                    |                      |                     |
 |   [使用者完成付款]   |                      |                     |
 |                    |                      |                     |
 |<-- 跳轉 confirmUrl -|<----- 使用者認證完成 ---------------|   |
 |                    | (URL包含 transactionId)                |   |
 |                    |                      |                     |
 |-- 呼叫確認 API ---->|                      |                     |
 |                    |-- POST /v3/payments/{transactionId}/confirm ->|
 |                    |   Body: {amount, currency}            |   |
 |                    |   Headers: {ChannelId, Signature, Nonce}  |
 |                    |                      |                     |
 |                    |<-- returnCode: 0000 -|                     |
 |                    |                      |                     |
 |                    |-- 更新 Order.Status = "Paid" ---------->|   |
 |                    |-- 更新 Ticket.Status = "Unused" ------->|   |
 |                    |-- 儲存 PaymentTransactionId ----------->|   |
 |                    |-- 生成 QR Code ------------------------>|   |
 |                    |                      |                     |
 |<-- 付款成功 --------|                      |                     |
 |   顯示票券          |                      |                     |
```

---

## 📝 9. 重點提醒清單

### ✅ 必須做的事

1. **正確生成 HMAC 簽章**
   - 嚴格按照規則組合訊息：`channelSecret + apiPath + body/queryString + nonce`
   - 使用 HMAC-SHA256 加密
   - 轉為 Base64 編碼

2. **處理交易 ID**
   - 在 C# 中使用 `long` 類型儲存
   - 儲存至資料庫時使用 `BIGINT` 或 `VARCHAR`

3. **金額驗證**
   - Request API 的 `amount` 必須與所有 products 的總和一致
   - Confirm API 的 `amount` 必須與 Request API 一致

4. **狀態更新**
   - Confirm API 成功後立即更新 `Order.Status` 和 `Ticket.Status`
   - 儲存 `PaymentTransactionId` 供未來對帳使用

### ⚠️ 常見錯誤

1. **簽章錯誤**
   - 訊息組合順序錯誤
   - 忘記加入 Channel Secret 前綴
   - 使用錯誤的加密演算法（必須是 HMAC-SHA256）

2. **金額不一致**
   - Request 和 Confirm 的金額不同
   - 商品總和與 package.amount 不符

3. **重複確認**
   - 同一個 transactionId 呼叫多次 Confirm API
   - 解決方式：在呼叫 Confirm API 前先檢查訂單狀態

4. **URL 設定錯誤**
   - `confirmUrl` 和 `cancelUrl` 必須是可公開存取的 HTTPS URL
   - Sandbox 環境可使用 HTTP（測試用）

---

## 🌐 10. 環境配置

### Sandbox 環境（測試）

| 項目 | 值 |
|------|-----|
| **API Base URL** | `https://sandbox-api-pay.line.me` |
| **Web 付款頁面** | `https://sandbox-web-pay.line.me` |
| **測試卡號** | 參考官方文檔的測試卡號清單 |

### Production 環境（正式）

| 項目 | 值 |
|------|-----|
| **API Base URL** | `https://api-pay.line.me` |
| **Web 付款頁面** | `https://web-pay.line.me` |

### appsettings.json 範例

```json
{
  "LinePay": {
    "ChannelId": "YOUR_CHANNEL_ID",
    "ChannelSecret": "YOUR_CHANNEL_SECRET",
    "IsSandbox": true,
    "ApiBaseUrl": "https://sandbox-api-pay.line.me",
    "ConfirmUrl": "https://yourdomain.com/api/payments/line-pay/confirm",
    "CancelUrl": "https://yourdomain.com/api/payments/line-pay/cancel"
  }
}
```

---

## 📚 11. 下一步

1. **申請 Sandbox 帳號**：取得測試用的 Channel ID 和 Secret
2. **實作簽章生成**：建立 `LinePaySignature` 類別
3. **封裝 HTTP Client**：建立 `LinePayHttpClient` 類別
4. **實作 Request API**：建立付款請求服務
5. **實作 Confirm API**：建立付款確認服務
6. **整合前端**：處理付款頁面跳轉與回呼
7. **測試流程**：使用 Sandbox 環境測試完整流程

---

**文檔版本：** v1.0  
**建立日期：** 2025-12-29  
**資料來源：** LINE Pay Online API v3 官方文檔
