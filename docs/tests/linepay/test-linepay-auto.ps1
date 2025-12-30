# LINE Pay API Testing Script
# This script automatically tests the complete LINE Pay flow

$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "LINE Pay Integration Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login to get Token
Write-Host "[Step 1] Logging in..." -ForegroundColor Yellow
$loginBody = @{
    email    = "test.customer@example.com"
    password = "Test123456!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json"
    
    $token = $loginResponse.data.token
    Write-Host "OK Login successful! Token: $($token.Substring(0, 20))..." -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "ERROR Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Create Order
Write-Host "[Step 2] Creating order..." -ForegroundColor Yellow
$orderBody = @{
    showTimeId = 7
    seatIds    = @(1, 2)
} | ConvertTo-Json

try {
    $orderResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" `
        -Method Post `
        -Headers @{ Authorization = "Bearer $token" } `
        -Body $orderBody `
        -ContentType "application/json"
    
    $orderId = $orderResponse.data.id
    $orderNumber = $orderResponse.data.orderNumber
    $totalPrice = $orderResponse.data.totalPrice
    
    Write-Host "OK Order created successfully!" -ForegroundColor Green
    Write-Host "  Order ID: $orderId" -ForegroundColor White
    Write-Host "  Order Number: $orderNumber" -ForegroundColor White
    Write-Host "  Total Price: $$totalPrice" -ForegroundColor White
    Write-Host ""
}
catch {
    $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "ERROR Order creation failed: $($errorDetails.message)" -ForegroundColor Red
    Write-Host "  Status Code: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    exit 1
}

# Step 3: Request LINE Pay Payment
Write-Host "[Step 3] Requesting LINE Pay payment..." -ForegroundColor Yellow
$paymentRequestBody = @{
    orderId = $orderId
} | ConvertTo-Json

try {
    $paymentRequestResponse = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" `
        -Method Post `
        -Headers @{ Authorization = "Bearer $token" } `
        -Body $paymentRequestBody `
        -ContentType "application/json"
    
    $transactionId = $paymentRequestResponse.data.transactionId
    $paymentUrl = $paymentRequestResponse.data.paymentUrl
    
    Write-Host "OK Payment request successful!" -ForegroundColor Green
    Write-Host "  Transaction ID: $transactionId" -ForegroundColor White
    Write-Host "  Payment URL: $paymentUrl" -ForegroundColor White
    Write-Host ""
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host "IMPORTANT: Please open the following URL in your browser:" -ForegroundColor Yellow
    Write-Host "   $paymentUrl" -ForegroundColor White
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host ""
    
}
catch {
    $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
    Write-Host "ERROR Payment request failed: $($errorDetails.message)" -ForegroundColor Red
    
    if ($errorDetails.detail) {
        Write-Host "  Error Detail: $($errorDetails.detail)" -ForegroundColor Red
    }
    
    Write-Host "  Status Code: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test completed up to payment request!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open the payment URL in browser" -ForegroundColor White
Write-Host "2. Complete payment in LINE Pay Sandbox" -ForegroundColor White
Write-Host "3. Run confirmation test separately" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
