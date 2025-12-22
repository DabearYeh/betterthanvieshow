# Simple test - just test create showtime
$baseUrl = "http://localhost:5041"

# Login
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Test Create Showtime
$showtimeBody = @{
    movieId   = 1
    theaterId = 1
    showDate  = "2025-12-25"
    startTime = "14:00"
} | ConvertTo-Json

Write-Host "Testing POST /api/admin/showtimes..."
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/showtimes" -Method POST -Headers $headers -Body $showtimeBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5)
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "FAILED (HTTP $statusCode)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        Write-Host $reader.ReadToEnd()
    }
}
