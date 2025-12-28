# Create Order API 測試腳本
$baseUrl = "http://localhost:5041"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試 1: 登入取得 Token" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$loginBody = @{
    email    = "test.order@example.com"
    password = "Test123456!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    $token = $loginResponse.data.token
    Write-Host "✅ 登入成功" -ForegroundColor Green
    Write-Host "Token: $($token.Substring(0, 50))..." -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "❌ 登入失敗: $_" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試 2: 成功建立訂單" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$orderBody = @{
    showTimeId = 7
    seatIds    = @(1, 2)
} | ConvertTo-Json

try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type"  = "application/json"
    }
    
    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers $headers -Body $orderBody
    Write-Host "✅ 訂單建立成功" -ForegroundColor Green
    Write-Host "訂單編號: $($orderResponse.data.orderNumber)" -ForegroundColor Yellow
    Write-Host "訂單狀態: $($orderResponse.data.status)" -ForegroundColor Yellow
    Write-Host "總金額: $($orderResponse.data.totalPrice) 元" -ForegroundColor Yellow
    Write-Host "票券數量: $($orderResponse.data.ticketCount) 張" -ForegroundColor Yellow
    Write-Host "付款期限: $($orderResponse.data.expiresAt)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "場次資訊:" -ForegroundColor Magenta
    Write-Host "  電影: $($orderResponse.data.showTime.movieTitle)" -ForegroundColor Gray
    Write-Host "  日期: $($orderResponse.data.showTime.showDate)" -ForegroundColor Gray
    Write-Host "  時間: $($orderResponse.data.showTime.startTime)" -ForegroundColor Gray
    Write-Host "  影廳: $($orderResponse.data.showTime.theaterName)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "票券清單:" -ForegroundColor Magenta
    foreach ($ticket in $orderResponse.data.tickets) {
        Write-Host "  座位 $($ticket.seatRow)$($ticket.seatColumn) - 票價: $($ticket.price) 元 - 狀態: $($ticket.status)" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "❌ 建立訂單失敗" -ForegroundColor Red
    Write-Host "錯誤: $_" -ForegroundColor Red
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試 3: 座位已被訂購（應該失敗）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$occupiedSeatBody = @{
    showTimeId = 7
    seatIds    = @(1, 3)  # 座位 1 已被上一個訂單訂購
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers $headers -Body $occupiedSeatBody
    Write-Host "⚠️ 預期應該失敗，但成功了" -ForegroundColor Yellow
}
catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($errorResponse.message -like "*已被訂購*") {
        Write-Host "✅ 正確拒絕已佔用座位" -ForegroundColor Green
        Write-Host "錯誤訊息: $($errorResponse.message)" -ForegroundColor Gray
    }
    else {
        Write-Host "❌ 錯誤訊息不符預期" -ForegroundColor Red
        Write-Host "實際訊息: $($errorResponse.message)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試 4: 訂購超過 6 張票（應該失敗）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$tooManySeatsBody = @{
    showTimeId = 7
    seatIds    = @(11, 12, 13, 14, 15, 16, 17)
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers $headers -Body $tooManySeatsBody
    Write-Host "⚠️ 預期應該失敗，但成功了" -ForegroundColor Yellow
}
catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($errorResponse.message -like "*最多可訂 6 張票*" -or $errorResponse.errors) {
        Write-Host "✅ 正確拒絕超過 6 張票" -ForegroundColor Green
        Write-Host "錯誤訊息: $($errorResponse.message)" -ForegroundColor Gray
    }
    else {
        Write-Host "❌ 錯誤訊息不符預期" -ForegroundColor Red
        Write-Host "實際訊息: $($errorResponse.message)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試 5: 場次不存在（應該失敗）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$invalidShowtimeBody = @{
    showTimeId = 999999
    seatIds    = @(11, 12)
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers $headers -Body $invalidShowtimeBody
    Write-Host "⚠️ 預期應該失敗，但成功了" -ForegroundColor Yellow
}
catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($errorResponse.message -like "*找不到*場次*") {
        Write-Host "✅ 正確拒絕不存在的場次" -ForegroundColor Green
        Write-Host "錯誤訊息: $($errorResponse.message)" -ForegroundColor Gray
    }
    else {
        Write-Host "❌ 錯誤訊息不符預期" -ForegroundColor Red
        Write-Host "實際訊息: $($errorResponse.message)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試 6: 未授權（應該失敗）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$unauthorizedBody = @{
    showTimeId = 7
    seatIds    = @(4, 5)
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -ContentType "application/json" -Body $unauthorizedBody
    Write-Host "⚠️ 預期應該失敗，但成功了" -ForegroundColor Yellow
}
catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✅ 正確要求授權" -ForegroundColor Green
        Write-Host "狀態碼: 401 Unauthorized" -ForegroundColor Gray
    }
    else {
        Write-Host "❌ 狀態碼不符預期" -ForegroundColor Red
        Write-Host "實際狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "測試完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
