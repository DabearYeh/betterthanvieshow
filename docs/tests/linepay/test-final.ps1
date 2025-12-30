# LINE Pay Integration Final Test
$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "LINE Pay Integration Test - Final Run" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Login
Write-Host "[1] Logging in..." -ForegroundColor Yellow
$loginBody = '{"email":"test.customer@example.com","password":"Test123456!"}'
try {
    $loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResp.data.token
    Write-Host "    OK - Token obtained`n" -ForegroundColor Green
}
catch {
    Write-Host "    FAILED`n" -ForegroundColor Red
    exit 1
}

# Step 2: Create Order with different seats
Write-Host "[2] Creating order (trying seats 10, 11)..." -ForegroundColor Yellow
$orderBody = '{"showTimeId":7,"seatIds":[10,11]}'
try {
    $orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body $orderBody -ContentType "application/json"
    $orderId = $orderResp.data.id
    $orderNum = $orderResp.data.orderNumber
    $price = $orderResp.data.totalPrice
    Write-Host "    OK - Order created" -ForegroundColor Green
    Write-Host "    Order ID: $orderId" -ForegroundColor White
    Write-Host "    Order Number: $orderNum" -ForegroundColor White
    Write-Host "    Total Price: `$$price`n" -ForegroundColor White
}
catch {
    Write-Host "    FAILED - Seats occupied, trying 20, 21..." -ForegroundColor Yellow
    $orderBody = '{"showTimeId":7,"seatIds":[20,21]}'
    try {
        $orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body $orderBody -ContentType "application/json"
        $orderId = $orderResp.data.id
        $orderNum = $orderResp.data.orderNumber
        $price = $orderResp.data.totalPrice
        Write-Host "    OK - Order created" -ForegroundColor Green
        Write-Host "    Order ID: $orderId" -ForegroundColor White
        Write-Host "    Order Number: $orderNum" -ForegroundColor White
        Write-Host "    Total Price: `$$price`n" -ForegroundColor White
    }
    catch {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errBody = $reader.ReadToEnd()
        Write-Host "    FAILED - $errBody`n" -ForegroundColor Red
        exit 1
    }
}

# Step 3: Request LINE Pay Payment
Write-Host "[3] Requesting LINE Pay payment..." -ForegroundColor Yellow
$paymentBody = "{`"orderId`":$orderId}"
try {
    $payResp = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" -Method Post -Headers @{Authorization = "Bearer $token" } -Body $paymentBody -ContentType "application/json"
    $txnId = $payResp.data.transactionId
    $payUrl = $payResp.data.paymentUrl
    
    Write-Host "    OK - Payment request successful!" -ForegroundColor Green
    Write-Host "    Transaction ID: $txnId" -ForegroundColor White
    Write-Host "    Payment URL: $payUrl`n" -ForegroundColor White
    
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " NEXT STEP: Open this URL in browser to complete payment:" -ForegroundColor Yellow
    Write-Host " $payUrl" -ForegroundColor White
    Write-Host "============================================================`n" -ForegroundColor Cyan
    
    Write-Host "Test Summary:" -ForegroundColor Cyan
    Write-Host "  - Login: SUCCESS" -ForegroundColor Green
    Write-Host "  - Create Order: SUCCESS (Order #$orderNum)" -ForegroundColor Green
    Write-Host "  - Payment Request: SUCCESS (TxnID: $txnId)" -ForegroundColor Green
    Write-Host "`nNext: Complete payment in browser, then test confirmation." -ForegroundColor Yellow
    
}
catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errBody = $reader.ReadToEnd()
    Write-Host "    FAILED`n" -ForegroundColor Red
    Write-Host "Error Details:" -ForegroundColor Yellow
    Write-Host $errBody -ForegroundColor White
    exit 1
}
