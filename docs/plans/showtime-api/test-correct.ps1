# Query and test with correct IDs
$baseUrl = "http://localhost:5041"

# Login
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token
$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Get first movie
$movies = Invoke-RestMethod -Uri "$baseUrl/api/admin/movies" -Method GET -Headers $headers
$movieId = $movies[0].id
$movieTitle = $movies[0].title
$releaseDate = $movies[0].releaseDate
$endDate = $movies[0].endDate
Write-Host "Movie: ID=$movieId, Title=$movieTitle"
Write-Host "  Release: $releaseDate, End: $endDate"

# Get first theater
$theaters = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method GET -Headers $headers
$theaterId = $theaters[0].id
$theaterName = $theaters[0].name
Write-Host "Theater: ID=$theaterId, Name=$theaterName"

# Now test create showtime with correct IDs
Write-Host "`n=== Testing Create Showtime ===" -ForegroundColor Green

$showtimeBody = @{
    movieId   = $movieId
    theaterId = $theaterId
    showDate  = "2025-12-30"
    startTime = "14:00"
} | ConvertTo-Json

Write-Host "Request body: $showtimeBody"

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
