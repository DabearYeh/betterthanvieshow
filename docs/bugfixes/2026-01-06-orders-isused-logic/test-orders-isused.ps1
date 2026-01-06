$baseUrl = "http://localhost:5041"

Write-Host "Testing GET /api/orders - isUsed field logic" -ForegroundColor Cyan
Write-Host ""

# Step 1: Login
Write-Host "[Step 1] Login..." -ForegroundColor Yellow
$loginBody = @{
    email    = "test1234@gmail.com"
    password = "Test1234"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"

if ($loginResponse.success) {
    $token = $loginResponse.data.token
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "Login failed: $($loginResponse.message)" -ForegroundColor Red
    exit 1
}

# Step 2: Get orders
Write-Host "[Step 2] Get orders..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
}

$ordersResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Get -Headers $headers

if ($ordersResponse.success) {
    Write-Host "Success! Total orders: $($ordersResponse.data.Count)" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Order Details:" -ForegroundColor Cyan
    Write-Host "----------------------------------------" -ForegroundColor Gray
    
    foreach ($order in $ordersResponse.data) {
        Write-Host "Order ID: $($order.orderId)" -ForegroundColor White
        Write-Host "  Movie: $($order.movieTitle)" -ForegroundColor White
        Write-Host "  Tickets: $($order.ticketCount)" -ForegroundColor White
        Write-Host "  Status: $($order.status)" -ForegroundColor White
        Write-Host "  isUsed: $($order.isUsed)" -ForegroundColor $(if ($order.isUsed) { "Red" } else { "Green" })
        Write-Host ""
    }
    
    Write-Host "Full JSON Response:" -ForegroundColor Gray
    $ordersResponse.data | ConvertTo-Json -Depth 5
    
}
else {
    Write-Host "Failed: $($ordersResponse.message)" -ForegroundColor Red
}
