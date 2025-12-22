# API Test Script

$baseUrl = "http://localhost:5041"

# 1. Login to get Token
Write-Host "=== 1. Login ===" -ForegroundColor Green
$loginBody = @{
    email    = "admin1234@gmail.com"
    password = "Admin1234"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
    Write-Host "Login SUCCESS!"
    # Token is in data.token
    $token = $loginResponse.data.token
    if ($token) {
        Write-Host "Token obtained (length: $($token.Length))"
    }
    else {
        Write-Host "Token is null!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "Login FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

# 2. Test Create Showtime
Write-Host "`n=== 2. Test Create Showtime ===" -ForegroundColor Green
$showtimeBody = @{
    movieId   = 1
    theaterId = 1
    showDate  = "2025-12-25"
    startTime = "14:00"
} | ConvertTo-Json

try {
    $showtimeResponse = Invoke-RestMethod -Uri "$baseUrl/api/admin/showtimes" -Method POST -Headers $headers -Body $showtimeBody
    Write-Host "Create Showtime SUCCESS!" -ForegroundColor Green
    Write-Host "Response:"
    $showtimeResponse | ConvertTo-Json -Depth 5
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Create Showtime FAILED (HTTP $statusCode)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error: $errorBody"
    }
}

# 3. Test - Movie not found (should fail with 400)
Write-Host "`n=== 3. Test Movie Not Found ===" -ForegroundColor Yellow
$showtimeBody = @{
    movieId   = 9999
    theaterId = 1
    showDate  = "2025-12-25"
    startTime = "14:00"
} | ConvertTo-Json

try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/showtimes" -Method POST -Headers $headers -Body $showtimeBody
    Write-Host "Result: SUCCESS (unexpected)" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

# 4. Test - Time not 15 min multiple (should fail with 400)
Write-Host "`n=== 4. Test Time Not 15min Multiple ===" -ForegroundColor Yellow
$showtimeBody = @{
    movieId   = 1
    theaterId = 1
    showDate  = "2025-12-25"
    startTime = "14:07"
} | ConvertTo-Json

try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/showtimes" -Method POST -Headers $headers -Body $showtimeBody
    Write-Host "Result: SUCCESS (unexpected)" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
