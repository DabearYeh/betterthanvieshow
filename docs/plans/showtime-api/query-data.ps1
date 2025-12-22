# Query existing movies and theaters
$baseUrl = "http://localhost:5041"

# Login
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

Write-Host "=== Fetching Movies ===" -ForegroundColor Green
try {
    $movies = Invoke-RestMethod -Uri "$baseUrl/api/admin/movies" -Method GET -Headers $headers
    Write-Host "Movies found: $($movies.Count)"
    $movies | ForEach-Object { Write-Host "  ID: $($_.id), Title: $($_.title), Release: $($_.releaseDate), End: $($_.endDate)" }
}
catch {
    Write-Host "Failed to fetch movies: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Fetching Theaters ===" -ForegroundColor Green
try {
    $theaters = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method GET -Headers $headers
    Write-Host "Theaters found: $($theaters.Count)"
    $theaters | ForEach-Object { Write-Host "  ID: $($_.id), Name: $($_.name), Type: $($_.type)" }
}
catch {
    Write-Host "Failed to fetch theaters: $($_.Exception.Message)" -ForegroundColor Red
}
