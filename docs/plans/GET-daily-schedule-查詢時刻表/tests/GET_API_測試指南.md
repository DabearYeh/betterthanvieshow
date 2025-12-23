# GET Daily Schedule API 測試指南

## 前置準備

應用程式目前執行在 `http://localhost:5041`

## 測試步驟

### 步驟 1：先執行登入取得 Token

在命令列執行：
```powershell
$loginResp = Invoke-RestMethod -Uri "http://localhost:5041/api/auth/login" -Method POST -Body '{"email":"admin@example.com","password":"Admin@123"}' -ContentType "application/json"
$loginResp.token
```

或在 VS Code 中使用 REST Client 擴展：
1. 開啟 `tests/get-daily-schedule.http`
2. 點擊第 7 行的 "Send Request"
3. 複製回傳的 token

### 步驟 2：替換 Token

將 `get-daily-schedule.http` 第 4 行的 `YOUR_ADMIN_TOKEN_HERE` 替換為剛才取得的 token

### 步驟 3：執行各項測試

依序執行以下測試（使用 VS Code REST Client 擴展）：

#### 測試 1：查詢不存在的時刻表
- 第 14-16 行
- 預期：返回 **404 Not Found**
- 錯誤訊息：「該日期沒有時刻表記錄」

#### 測試 2：日期格式錯誤
- 第 18-20 行
- 預期：返回 **400 Bad Request**
- 錯誤訊息：「日期格式錯誤，必須為 YYYY-MM-DD」

#### 測試 3：未授權訪問
- 第 22-23 行（此行沒有 Authorization header）
- 預期：返回 **401 Unauthorized**

#### 測試 4 & 5：查詢存在的時刻表
- 第 6-8 行 和 10-12 行
- **注意**：這需要先在資料庫中建立 2025-12-24 和 2025-12-25 的時刻表資料
- 預期：返回 **200 OK** 及完整的時刻表 JSON

## 如何建立測試資料

使用現有的 `PUT /api/admin/daily-schedules/{date}` API建立時刻表：

```json
PUT http://localhost:5041/api/admin/daily-schedules/2025-12-24
Authorization: Bearer YOUR_TOKEN

{
  "showtimes": [
    {
      "movieId": 1,
      "theaterId": 1,
      "startTime": "09:00"
    }
  ]
}
```

## 預期回應範例

```json
{
  "scheduleDate": "2025-12-24T00:00:00",
  "status": "Draft",
  "showtimes": [
    {
      "id": 1,
      "movieId": 1,
      "movieTitle": "電影名稱",
      "movieDuration": 120,
      "theaterId": 1,
      "theaterName": "影廳名稱",
      "theaterType": "IMAX",
      "showDate": "2025-12-24T00:00:00",
      "startTime": "09:00",
      "endTime": "11:00",
      "scheduleStatus": "Draft",
      "createdAt": "2025-12-23T10:00:00Z"
    }
  ],
  "createdAt": "2025-12-23T10:00:00Z",
  "updatedAt": "2025-12-23T10:00:00Z"
}
```

## 故障排除

如果遇到 500 錯誤，請檢查：
1. 資料庫連線是否正常
2. 應用程式日誌中的錯誤訊息
3. 確保 admin@example.com 使用者已存在於資料庫中
