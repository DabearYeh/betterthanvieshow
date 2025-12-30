# Final Diagnosis - Check Order Response Structure
$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

# Login
$loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body '{"email":"test.customer@example.com","password":"Test123456!"}' -ContentType "application/json"
$token = $loginResp.data.token

# Create Order and examine full response
Write-Host "Creating order and examining response structure..." -ForegroundColor Cyan
$orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body '{"showTimeId":7,"seatIds":[7,8]}' -ContentType "application/json"

Write-Host "`nFull Order Response:" -ForegroundColor Yellow
$orderResString = $orderResp | ConvertTo-Json -Depth 10
Write-Host $orderResString

Write-Host "`n`nChecking specific fields:" -ForegroundColor Yellow
Write-Host "  orderResp.data.id = '$($orderResp.data.id)'" -ForegroundColor White
Write-Host "  orderResp.data.orderNumber = '$($orderResp.data.orderNumber)'" -ForegroundColor White
Write-Host "  orderResp.data.totalPrice = '$($orderResp.data.totalPrice)'" -ForegroundColor White

if ($orderResp.data.id) {
    Write-Host "`n  OrderId exists! Value: $($orderResp.data.id)" -ForegroundColor Green
    $orderId = $orderResp.data.id
    
    # Try LINE Pay with correct OrderId
    Write-Host "`nAttempting LINE Pay request with OrderId = $orderId" -ForegroundColor Cyan
    try {
        $payResp = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" -Method Post -Headers @{Authorization = "Bearer $token" } -Body "{`"orderId`":$orderId}" -ContentType "application/json"
        Write-Host "SUCCESS!" -ForegroundColor Green
        Write-Host "  Transaction ID: $($payResp.data.transactionId)" -ForegroundColor White
        Write-Host "  Payment URL: $($payResp.data.paymentUrl)" -ForegroundColor White
    }
    catch {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "FAILED:" -ForegroundColor Red
        Write-Host $reader.ReadToEnd() -ForegroundColor White
    }
}
else {
    Write-Host "`n  OrderId is NULL or missing!" -ForegroundColor Red
}
