# Simple test to find available seats and test LINE Pay

$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

Write-Host "LINE Pay Integration Test`n" -ForegroundColor Cyan

# Login
Write-Host "[Step 1] Login..." -ForegroundColor Yellow
$loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body '{"email":"test.customer@example.com","password":"Test123456!"}' -ContentType "application/json"
$token = $loginResp.data.token
Write-Host "  OK`n" -ForegroundColor Green

# Try creating order with seat IDs 3, 4 (likely available)
Write-Host "[Step 2] Create Order (seats 3, 4)..." -ForegroundColor Yellow
try {
    $orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body '{"showTimeId":7,"seatIds":[3,4]}' -ContentType "application/json"
    $orderId = $orderResp.data.id
    Write-Host "  OK - Order #$($orderResp.data.orderNumber), ID: $orderId, Price: `$$($orderResp.data.totalPrice)`n" -ForegroundColor Green
    
    # Request Payment
    Write-Host "[Step 3] Request LINE Pay..." -ForegroundColor Yellow
    try {
        $payResp = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" -Method Post -Headers @{Authorization = "Bearer $token" } -Body "{`"orderId`":$orderId}" -ContentType "application/json"
        
        Write-Host "  OK`n" -ForegroundColor Green
        Write-Host "================================================================" -ForegroundColor Green
        Write-Host " SUCCESS! LINE Pay Integration is WORKING!" -ForegroundColor Green
        Write-Host "================================================================" -ForegroundColor Green
        Write-Host "`nTransaction ID: $($payResp.data.transactionId)" -ForegroundColor White
        Write-Host "Payment URL: $($payResp.data.paymentUrl)" -ForegroundColor Cyan
        Write-Host "`nTo complete the test:" -ForegroundColor Yellow
        Write-Host "1. Open the payment URL in your browser" -ForegroundColor White
        Write-Host "2. Complete payment in LINE Pay Sandbox" -ForegroundColor White
        Write-Host "3. Payment confirmation will update order status to 'Paid'" -ForegroundColor White
    }
    catch {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "  FAILED`n" -ForegroundColor Red
        Write-Host $reader.ReadToEnd() -ForegroundColor Yellow
    }
    
}
catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    Write-Host "  FAILED`n" -ForegroundColor Red
    Write-Host $reader.ReadToEnd() -ForegroundColor Yellow
}
