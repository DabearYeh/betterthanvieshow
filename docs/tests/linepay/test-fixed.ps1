#  LINE Pay Complete Test - Fixed Version
$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "LINE Pay Integration Final Test" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Step 1: Login
Write-Host "[1] Login..." -ForegroundColor Yellow
$loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body '{"email":"test.customer@example.com","password":"Test123456!"}' -ContentType "application/json"
$token = $loginResp.data.token
Write-Host "  OK`n" -ForegroundColor Green

# Step 2: Create Order
Write-Host "[2] Create Order..." -ForegroundColor Yellow
$orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body '{"showTimeId":7,"seatIds":[9,10]}' -ContentType "application/json"
$orderId = $orderResp.data.orderId  # FIXED: use orderId not id
Write-Host "  OK - Order #$($orderResp.data.orderNumber), ID: $orderId, Price: `$$($orderResp.data.totalPrice)`n" -ForegroundColor Green

# Step 3: Request LINE Pay Payment
Write-Host "[3] Request LINE Pay Payment..." -ForegroundColor Yellow
try {
    $payResp = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" -Method Post -Headers @{Authorization = "Bearer $token" } -Body "{`"orderId`":$orderId}" -ContentType "application/json"
    
    Write-Host "  OK`n" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host " SUCCESS! LINE Pay Integration is WORKING!" -ForegroundColor Green
    Write-Host "============================================================`n" -ForegroundColor Green
    
    Write-Host "Transaction ID: $($payResp.data.transactionId)" -ForegroundColor White
    Write-Host "Payment URL:`n$($payResp.data.paymentUrl)`n" -ForegroundColor Cyan
    
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Open the payment URL above in your browser" -ForegroundColor White
    Write-Host "2. Log in with LINE Pay Sandbox test account" -ForegroundColor White
    Write-Host "3. Complete the payment" -ForegroundColor White
    Write-Host "4. You will be redirected back to your frontend" -ForegroundColor White
    
}
catch {
    Write-Host "  FAILED`n" -ForegroundColor Red
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errBody = $reader.ReadToEnd()
    Write-Host "Error Details:" -ForegroundColor Yellow
    Write-Host $errBody -ForegroundColor White
}
