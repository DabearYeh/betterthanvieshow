# LINE Pay 金流整合文檔索引

## 📚 文檔總覽

本資料夾包含 LINE Pay 金流整合的完整文檔，從分析到實作的所有資訊。

---

## 📄 文檔清單

### 1. [完整付款流程說明.md](./完整付款流程說明.md) ⭐ 推薦先讀
**目的：** 以白話文方式說明從使用者選座到付款完成的完整流程

**內容包含：**
- 🎬 第零階段：建立訂單（座位鎖定）
- 🔄 第一階段：準備付款（取得付款網址）
- ⏸️ 中場休息：使用者在 LINE Pay 操作
- ✅ 第二階段：確認付款（更新訂單狀態）
- ⚠️ 異常情況處理（取消、超時、失敗）
- 📋 技術重點回顧

**適合閱讀時機：** 第一次了解完整流程，或需要向他人說明流程時

---

### 2. [流程分析.md](./流程分析.md)
**目的：** 分析 LINE Pay 付款流程圖與專案規格的契合度

**內容包含：**
- ✅ 符合專案規格的部分
- ⚠️ 需要調整與新增的部分
- 🎯 需要新增的功能清單
- 📋 實作優先順序（Phase 1-3）
- 🔧 設定檔案配置範例
- 📊 完整流程時序圖

**適合閱讀時機：** 開始開發前，了解整體架構與需求

---

### 3. [LINE_Pay_官方API文檔整理.md](./LINE_Pay_官方API文檔整理.md)
**目的：** LINE Pay Online API v3 官方文檔的完整整理與中文化

**內容包含：**
- 🔐 認證機制 (HMAC-SHA256 簽章)
- 📤 付款請求 API (Request API) 詳細說明
- 🔐 LINE Pay 使用者認證流程
- ✅ 付款授權 API (Confirm API) 詳細說明
- 🛠️ C# 實作參考程式碼
- 📊 結果代碼參考
- 🔄 完整流程時序圖（含技術細節）
- 📝 重點提醒與常見錯誤

**適合閱讀時機：** 實際撰寫程式碼時，查閱 API 規格與範例

---

## 🚀 快速開始指南

### 第一步：了解整體架構
閱讀 **[流程分析.md](./流程分析.md)** 的以下章節：
1. 符合專案規格的部分
2. 需要調整與新增的部分
3. 實作優先順序

### 第二步：準備開發環境
1. 申請 [LINE Pay Sandbox 帳號](https://pay.line.me/tw/developers/techsupport/sandbox/testflow?locale=zh_TW)
2. 取得 Channel ID 和 Channel Secret
3. 在 `appsettings.json` 中配置憑證

### 第三步：實作核心功能
參考 **[LINE_Pay_官方API文檔整理.md](./LINE_Pay_官方API文檔整理.md)** 的以下章節：
1. 認證機制 → 實作簽章生成
2. 付款請求 API → 實作 Request 服務
3. 付款授權 API → 實作 Confirm 服務

### 第四步：整合與測試
1. 整合前端付款頁面跳轉
2. 實作 confirmUrl 和 cancelUrl 端點
3. 在 Sandbox 環境測試完整流程

---

## 🔑 關鍵概念速查

### HMAC 簽章生成公式

#### POST 方法
```
MAC訊息 = channelSecret + apiPath + requestBody(JSON字串) + nonce
簽章 = HMAC-SHA256(MAC訊息, channelSecret) → Base64編碼
```

#### GET 方法
```
MAC訊息 = channelSecret + apiPath + queryString + nonce
簽章 = HMAC-SHA256(MAC訊息, channelSecret) → Base64編碼
```

### API 端點速查

| API | 方法 | 路徑 | 用途 |
|-----|------|------|------|
| Request | POST | `/v3/payments/request` | 發起付款請求 |
| Confirm | POST | `/v3/payments/{transactionId}/confirm` | 確認付款完成 |

### 訂單狀態流轉

```
Pending (未付款)
   ↓ [完成 LINE Pay 付款 + Confirm API 成功]
Paid (已付款)
   ↓ [5分鐘未付款]
Cancelled (已取消)
```

### 票券狀態流轉

```
Pending (待支付) → 座位鎖定
   ↓ [付款成功]
Unused (未使用)
   ↓ [掃描 QR Code]
Used (已使用)

Pending (待支付)
   ↓ [5分鐘未付款 或 電影結束]
Expired (已過期) → 座位釋放
```

---

## 📊 實作檢查清單

### Phase 1: 核心付款流程 ✅

- [ ] **設定開發環境**
  - [ ] 申請 LINE Pay Sandbox 帳號
  - [ ] 取得 Channel ID 和 Channel Secret
  - [ ] 在 `appsettings.json` 配置憑證

- [ ] **實作簽章生成**
  - [ ] 創建 `LinePaySignature` 類別
  - [ ] 實作 `GenerateSignature()` 方法
  - [ ] 實作 `GenerateNonce()` 方法
  - [ ] 撰寫單元測試驗證簽章正確性

- [ ] **封裝 HTTP Client**
  - [ ] 創建 `LinePayHttpClient` 類別
  - [ ] 實作 `PostAsync()` 方法（自動加入簽章標頭）
  - [ ] 實作 `GetAsync()` 方法（自動加入簽章標頭）
  - [ ] 註冊至 DI 容器

- [ ] **實作 Request API**
  - [ ] 創建 `IPaymentService` 介面
  - [ ] 創建 `LinePayService` 實作類別
  - [ ] 實作 `CreatePaymentRequestAsync()` 方法
  - [ ] 定義 Request/Response DTO
  - [ ] 撰寫單元測試

- [ ] **實作 Confirm API**
  - [ ] 實作 `ConfirmPaymentAsync()` 方法
  - [ ] 定義 Request/Response DTO
  - [ ] 實作訂單狀態更新邏輯
  - [ ] 實作票券狀態更新邏輯
  - [ ] 撰寫單元測試

- [ ] **創建 API 端點**
  - [ ] 創建 `PaymentsController`
  - [ ] 新增 `POST /api/payments/line-pay/request`
  - [ ] 新增 `POST /api/payments/line-pay/confirm`
  - [ ] 新增 `GET /api/payments/line-pay/cancel`
  - [ ] 撰寫 Swagger 文檔

- [ ] **整合測試**
  - [ ] 測試 Request API 呼叫
  - [ ] 測試付款頁面跳轉
  - [ ] 測試使用者完成付款流程
  - [ ] 測試 Confirm API 呼叫
  - [ ] 驗證訂單狀態正確更新
  - [ ] 驗證票券狀態正確更新

### Phase 2: 自動化處理 ⏳

- [ ] **背景服務 - 訂單過期**
  - [ ] 創建 `OrderExpirationService : BackgroundService`
  - [ ] 實作每分鐘掃描過期訂單
  - [ ] 實作自動取消訂單邏輯
  - [ ] 實作自動釋放座位邏輯
  - [ ] 整合 SignalR 通知前端
  - [ ] 撰寫整合測試

- [ ] **錯誤處理與重試**
  - [ ] 實作 LINE Pay API 錯誤處理
  - [ ] 實作網路逾時重試機制
  - [ ] 實作異常狀態記錄（Log）
  - [ ] 建立錯誤通知機制

### Phase 3: 進階功能 ⭕

- [ ] **付款記錄查詢**
  - [ ] 管理員查詢所有付款記錄
  - [ ] 使用者查詢自己的付款歷史

- [ ] **對帳功能**
  - [ ] 使用 `PaymentTransactionId` 進行對帳
  - [ ] 匯出交易報表

---

## 🔗 相關資源

### 官方文檔
- [LINE Pay 開發者中心](https://pay.line.me/tw/developers?locale=zh_TW)
- [Online API v3 文檔](https://pay.line.me/tw/developers/apis/onlineApis?locale=zh_TW)
- [Sandbox 測試指南](https://pay.line.me/tw/developers/techsupport/sandbox/testflow?locale=zh_TW)

### 專案規格
- `docs/spec/erm.dbml` - 資料模型定義
- `docs/spec/features/訂票.feature` - 訂票功能規格

### 現有實作
- `betterthanvieshow/Models/Entities/Order.cs` - 訂單實體
- `betterthanvieshow/Models/Entities/Ticket.cs` - 票券實體
- `betterthanvieshow/Services/Implementations/OrderService.cs` - 訂單服務

---

## 💡 常見問題 FAQ

### Q1: 如何取得 Sandbox 測試帳號？
**A:** 前往 [LINE Pay Sandbox 申請頁面](https://pay.line.me/tw/developers/techsupport/sandbox/creation?locale=zh_TW)，填寫表單即可取得測試用的 Channel ID 和 Channel Secret。

### Q2: HMAC 簽章一直失敗怎麼辦？
**A:** 檢查以下項目：
1. 訊息組合順序是否正確（channelSecret + apiPath + body/queryString + nonce）
2. 是否使用正確的加密演算法（HMAC-SHA256）
3. requestBody 是否為正確的 JSON 字串格式（不含空白）
4. nonce 是否正確傳入標頭

### Q3: Confirm API 回傳錯誤代碼 1124（金額錯誤）？
**A:** 確認 Confirm API 的 `amount` 和 `currency` 是否與 Request API 完全一致。

### Q4: 如何處理使用者取消付款的情況？
**A:** 使用者在 LINE Pay 頁面取消時，會跳轉至您設定的 `cancelUrl`。您可以在該端點記錄取消事件，並導向前端的取消頁面。訂單會在 5 分鐘後自動過期。

### Q5: transactionId 在 C# 中應該用什麼類型？
**A:** 使用 `long` 類型即可安全處理，資料庫欄位使用 `BIGINT` 或 `VARCHAR(20)`。

### Q6: 是否需要手動請款？
**A:** 預設情況下，Confirm API 完成後會自動請款。如需分開授權與請款，需使用不同的 API（請參考官方文檔的「預先授權」功能）。

---

## 📞 聯絡支援

如有任何問題，請參考：
- LINE Pay 官方技術支援：[技術支援頁面](https://pay.line.me/tw/developers/techsupport/overview?locale=zh_TW)
- LINE Pay 開發者論壇：[開發者社群](https://developers.line.biz/zh-hant/)

---

**文檔版本：** v1.0  
**建立日期：** 2025-12-29  
**最後更新：** 2025-12-29
