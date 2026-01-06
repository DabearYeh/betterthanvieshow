# Quick test
$body = @{ email = "test1234@gmail.com"; password = "Test1234" } | ConvertTo-Json
$loginResp = Invoke-RestMethod -Uri "http://localhost:5041/api/auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $loginResp.data.token

$headers = @{ Authorization = "Bearer $token" }
$ordersResp = Invoke-RestMethod -Uri "http://localhost:5041/api/orders" -Method Get -Headers $headers

Write-Host "=== Orders Test Result ===" -ForegroundColor Cyan
$ordersResp.data | ForEach-Object {
    Write-Host "Order $($_.orderId): $($_.movieTitle)" -ForegroundColor Yellow
    Write-Host "  Tickets: $($_.ticketCount), Status: $($_.status), isUsed: $($_.isUsed)" -ForegroundColor White
}
