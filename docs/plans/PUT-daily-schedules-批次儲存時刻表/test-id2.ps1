# Test with ID=2
$baseUrl = "http://localhost:5041"

# Login
Write-Host "=== Login ===" -ForegroundColor Green
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token
Write-Host "Login SUCCESS! Token length: $($token.Length)"

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Test 1: Save daily schedule with ID=2
Write-Host "`n=== Test 1: Save Daily Schedule (movieId=2, theaterId=2) ===" -ForegroundColor Green
$requestBody = @{
    showtimes = @(
        @{ movieId = 2; theaterId = 2; startTime = "09:45" },
        @{ movieId = 2; theaterId = 2; startTime = "14:00" }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Schedule Date: $($response.scheduleDate)"
    Write-Host "Status: $($response.status)"
    Write-Host "Showtimes Count: $($response.showtimes.Count)"
    $response.showtimes | ForEach-Object { 
        Write-Host "  - $($_.movieTitle) @ $($_.theaterName), $($_.startTime)-$($_.endTime)" 
    }
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        Write-Host $reader.ReadToEnd()
    }
}

# Test 2: Modify (replace with 1 showtime)
Write-Host "`n=== Test 2: Modify Schedule (replace with 1 showtime) ===" -ForegroundColor Green
$requestBody = @{
    showtimes = @(
        @{ movieId = 2; theaterId = 2; startTime = "10:00" }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Showtimes Count: $($response.showtimes.Count) (should be 1)"
    $response.showtimes | ForEach-Object { 
        Write-Host "  - $($_.movieTitle) @ $($_.theaterName), $($_.startTime)-$($_.endTime)" 
    }
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
}

# Test 3: Clear schedule
Write-Host "`n=== Test 3: Clear Schedule ===" -ForegroundColor Yellow
$requestBody = @{ showtimes = @() } | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Showtimes Count: $($response.showtimes.Count) (should be 0)"
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
