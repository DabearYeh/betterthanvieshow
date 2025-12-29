# GET /api/orders/{id} API Test Script
# Encoding: UTF-8

$baseUrl = "http://localhost:5041"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GET /api/orders/{id} API Testing" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Login
Write-Host "[Step 1] Logging in..." -ForegroundColor Yellow
$loginBody = '{"email":"test1234@gmail.com","password":"Test1234"}'

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Headers @{'Content-Type' = 'application/json' } -Body $loginBody
    $token = $loginResponse.data.token
    Write-Host "Login successful!`n" -ForegroundColor Green
}
catch {
    Write-Host "Login failed: $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    'Authorization' = "Bearer $token"
    'Content-Type'  = 'application/json'
}

# Step 2: Create test order  
Write-Host "[Step 2] Creating test order..." -ForegroundColor Yellow
$createBody = '{"showTimeId":7,"seatIds":[91,92]}'

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers $headers -Body $createBody
    $testOrderId = $createResponse.data.orderId
    Write-Host "Test order created: ID = $testOrderId`n" -ForegroundColor Green
}
catch {
    Write-Host "Could not create order (seats may be occupied). Using order ID 1 for testing.`n" -ForegroundColor Yellow
    $testOrderId = 1
}

# Test Scenario 1: Get order details (normal case)
Write-Host "[Scenario 1] Get order details (GET /api/orders/$testOrderId)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders/$testOrderId" -Method Get -Headers $headers
    
    if ($response.success -and $response.data.orderId) {
        Write-Host "PASS: Order details retrieved successfully" -ForegroundColor Green
        Write-Host "  Order Number: $($response.data.orderNumber)" -ForegroundColor White
        Write-Host "  Movie: $($response.data.movie.title)" -ForegroundColor White
        Write-Host "  Showtime: $($response.data.showtime.date) ($($response.data.showtime.dayOfWeek)) $($response.data.showtime.startTime)" -ForegroundColor White
        Write-Host "  Theater: $($response.data.theater.name) - $($response.data.theater.type)" -ForegroundColor White
        Write-Host "  Seats: $($response.data.seats.Count)" -ForegroundColor White
        Write-Host "  Total Amount: `$$($response.data.totalAmount)`n" -ForegroundColor White
    }
    else {
        Write-Host "FAIL: Unexpected response structure" -ForegroundColor Red
    }
}
catch {
    Write-Host "FAIL: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test Scenario 2: Non-existent order
Write-Host "[Scenario 2] Query non-existent order (GET /api/orders/99999)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders/99999" -Method Get -Headers $headers
    Write-Host "FAIL: Expected 404, got success" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 404) {
        Write-Host "PASS: Correctly returned 404 Not Found`n" -ForegroundColor Green
    }
    else {
        Write-Host "FAIL: Expected 404, got HTTP $statusCode`n" -ForegroundColor Red
    }
}

# Test Scenario 3: Unauthorized (no token)
Write-Host "[Scenario 3] Query without token" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders/$testOrderId" -Method Get
    Write-Host "FAIL: Expected 401, got success" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401) {
        Write-Host "PASS: Correctly returned 401 Unauthorized`n" -ForegroundColor Green
    }
    else {
        Write-Host "FAIL: Expected 401, got HTTP $statusCode`n" -ForegroundColor Red
    }
}

# Test Scenario 4: Invalid token
Write-Host "[Scenario 4] Query with invalid token" -ForegroundColor Yellow
$badHeaders = @{
    'Authorization' = "Bearer invalid.token.here"
    'Content-Type'  = 'application/json'
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/orders/$testOrderId" -Method Get -Headers $badHeaders
    Write-Host "FAIL: Expected 401, got success" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401) {
        Write-Host "PASS: Correctly returned 401 Unauthorized`n" -ForegroundColor Green
    }
    else {
        Write-Host "FAIL: Expected 401, got HTTP $statusCode`n" -ForegroundColor Red
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
