# LINE Pay API Detailed Test
$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

Write-Host "Testing LINE Pay API - Detailed Error Reporting" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login
Write-Host "[1] Login..." -ForegroundColor Yellow
$loginBody = '{"email":"test.customer@example.com","password":"Test123456!"}'
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "  OK - Token obtained" -ForegroundColor Green
}
catch {
    Write-Host "  FAILED - $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Create Order
Write-Host "[2] Create Order..." -ForegroundColor Yellow
$orderBody = '{"showTimeId":7,"seatIds":[1,2]}'
try {
    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body $orderBody -ContentType "application/json"
    $orderId = $orderResponse.data.id
    Write-Host "  OK - Order ID: $orderId, Number: $($orderResponse.data.orderNumber)" -ForegroundColor Green
}
catch {
    Write-Host "  FAILED" -ForegroundColor Red
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $respBody = $reader.ReadToEnd()
    Write-Host "  Error: $respBody" -ForegroundColor Red
    exit 1
}

# Step 3: Request LINE Pay Payment with detailed error
Write-Host "[3] Request LINE Pay Payment..." -ForegroundColor Yellow
$paymentBody = "{`"orderId`":$orderId}"
try {
    $paymentResponse = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" -Method Post -Headers @{Authorization = "Bearer $token" } -Body $paymentBody -ContentType "application/json"
    Write-Host "  OK - Transaction ID: $($paymentResponse.data.transactionId)" -ForegroundColor Green
    Write-Host "  Payment URL: $($paymentResponse.data.paymentUrl)" -ForegroundColor Cyan
}
catch {
    Write-Host "  FAILED - Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errorBody = $reader.ReadToEnd()
    Write-Host "  Response Body:" -ForegroundColor Yellow
    Write-Host "  $errorBody" -ForegroundColor White
    exit 1
}
