# Test Publish Daily Schedule API
$baseUrl = "http://localhost:5041"

# Login
Write-Host "=== 1. Login ===" -ForegroundColor Green
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token
Write-Host "Login SUCCESS! Token length: $($token.Length)"

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Test 1: Save draft schedule first
Write-Host "`n=== 2. Save Draft Schedule (2025-12-31) ===" -ForegroundColor Green
$saveBody = @{
    showtimes = @(
        @{ movieId = 2; theaterId = 2; startTime = "10:00" }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31" -Method PUT -Headers $headers -Body $saveBody
    Write-Host "SUCCESS! Status: $($response.status)"
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
}

# Test 2: Publish (Draft → OnSale)
Write-Host "`n=== 3. Publish Schedule (Draft → OnSale) ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31/publish" -Method POST -Headers $headers
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Schedule Date: $($response.scheduleDate)"
    Write-Host "Status: $($response.status) (should be OnSale)"
    Write-Host "Showtimes Count: $($response.showtimes.Count)"
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host $_.Exception.Message
}

# Test 3: Try to publish again (Idempotency)
Write-Host "`n=== 4. Publish Again (Idempotency Test) ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31/publish" -Method POST -Headers $headers
    Write-Host "SUCCESS! (Idempotent)" -ForegroundColor Green
    Write-Host "Status: $($response.status) (still OnSale)"
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
}

# Test 4: Try to save after publish (should fail)
Write-Host "`n=== 5. Try to Save After Publish (Should Fail) ===" -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-31" -Method PUT -Headers $headers -Body $saveBody
    Write-Host "Result: SUCCESS (unexpected!)" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

# Test 5: Publish non-existent date
Write-Host "`n=== 6. Publish Non-Existent Date ===" -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2099-01-01/publish" -Method POST -Headers $headers
    Write-Host "Result: SUCCESS (unexpected!)" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "Result: FAILED (HTTP $statusCode) - Expected!" -ForegroundColor Green
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
