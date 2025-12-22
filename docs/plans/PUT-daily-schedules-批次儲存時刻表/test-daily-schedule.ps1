# Test PUT /api/admin/daily-schedules/{date} API
$baseUrl = "http://localhost:5041"

# 1. Login
Write-Host "=== 1. Login ===" -ForegroundColor Green
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token
Write-Host "Login SUCCESS! Token length: $($token.Length)"

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# 2. Test - Save daily schedule (first time)
Write-Host "`n=== 2. Test Save Daily Schedule (First Time) ===" -ForegroundColor Green
$requestBody = @{
    showtimes = @(
        @{ movieId = 1; theaterId = 1; startTime = "09:45" },
        @{ movieId = 1; theaterId = 2; startTime = "14:00" }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Schedule Date: $($response.scheduleDate)"
    Write-Host "Status: $($response.status)"
    Write-Host "Showtimes Count: $($response.showtimes.Count)"
    $response.showtimes | ForEach-Object { Write-Host "  - Movie: $($_.movieTitle), Theater: $($_.theaterName), Time: $($_.startTime)" }
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message
}

# 3. Test - Modify existing schedule
Write-Host "`n=== 3. Test Modify Existing Schedule ===" -ForegroundColor Green
$requestBody = @{
    showtimes = @(
        @{ movieId = 2; theaterId = 1; startTime = "10:00" }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Showtimes Count (should be 1): $($response.showtimes.Count)"
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message
}

# 4. Test - Clear schedule (empty array)
Write-Host "`n=== 4. Test Clear Schedule ===" -ForegroundColor Yellow
$requestBody = @{ showtimes = @() } | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Showtimes Count (should be 0): $($response.showtimes.Count)"
} catch {
    Write-Host "FAILED" -ForegroundColor Red
}

# 5. Test - Movie not found
Write-Host "`n=== 5. Test Movie Not Found ===" -ForegroundColor Yellow
$requestBody = @{
    showtimes = @( @{ movieId = 9999; theaterId = 1; startTime = "10:00" } )
} | ConvertTo-Json -Depth 3

try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "Result: SUCCESS (unexpected)" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

# 6. Test - Time not 15 min multiple
Write-Host "`n=== 6. Test Time Not 15min Multiple ===" -ForegroundColor Yellow
$requestBody = @{
    showtimes = @( @{ movieId = 1; theaterId = 1; startTime = "10:07" } )
} | ConvertTo-Json -Depth 3

try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "Result: SUCCESS (unexpected)" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

# 7. Test - Time conflict
Write-Host "`n=== 7. Test Time Conflict ===" -ForegroundColor Yellow
$requestBody = @{
    showtimes = @(
        @{ movieId = 1; theaterId = 1; startTime = "10:00" },
        @{ movieId = 1; theaterId = 1; startTime = "10:30" }
    )
} | ConvertTo-Json -Depth 3

try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "Result: SUCCESS (unexpected)" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
