# Query correct IDs
$baseUrl = "http://localhost:5041"

# Login
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Get movies
Write-Host "=== Movies ===" -ForegroundColor Green
$movies = Invoke-RestMethod -Uri "$baseUrl/api/admin/movies" -Method GET -Headers $headers
$movies | ForEach-Object { Write-Host "ID: $($_.id), Title: $($_.title), Release: $($_.releaseDate), End: $($_.endDate)" }

$movieId = $movies[0].id

# Get theaters
Write-Host "`n=== Theaters ===" -ForegroundColor Green
$theaters = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method GET -Headers $headers
$theaters | ForEach-Object { Write-Host "ID: $($_.id), Name: $($_.name)" }

$theaterId = $theaters[0].id

# Test with correct IDs
Write-Host "`n=== Testing with Movie ID: $movieId, Theater ID: $theaterId ===" -ForegroundColor Cyan

$requestBody = @{
    showtimes = @(
        @{ movieId = $movieId; theaterId = $theaterId; startTime = "09:45" }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Schedule Date: $($response.scheduleDate)"
    Write-Host "Status: $($response.status)"
    Write-Host "Showtimes: $($response.showtimes.Count)"
    $response.showtimes | ForEach-Object { Write-Host "  - $($_.movieTitle) @ $($_.theaterName), $($_.startTime)-$($_.endTime)" }
}
catch {
    Write-Host "FAILED" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        Write-Host $reader.ReadToEnd()
    }
}
