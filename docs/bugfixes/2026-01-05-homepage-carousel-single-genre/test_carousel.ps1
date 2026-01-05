# 簡單測試 Carousel Genre

$response = Invoke-RestMethod -Uri "http://localhost:5041/api/movies/homepage"

Write-Host "===== Carousel Movies =====" -ForegroundColor Cyan
foreach ($movie in $response.data.carousel) {
    Write-Host "Movie: $($movie.title)" -ForegroundColor White
    Write-Host "Genre: $($movie.genre)" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "===== TopWeekly Sample =====" -ForegroundColor Cyan
if ($response.data.topWeekly -and $response.data.topWeekly.Count -gt 0) {
    $sample = $response.data.topWeekly[0]
    Write-Host "Movie: $($sample.title)" -ForegroundColor White
    Write-Host "Genre: $($sample.genre)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "===== Validation =====" -ForegroundColor Cyan
$allSingle = $true
foreach ($movie in $response.data.carousel) {
    $count = ($movie.genre -split ',').Count
    if ($count -gt 1) {
        $allSingle = $false
        break
    }
}

if ($allSingle) {
    Write-Host "PASS - All carousel movies have single genre" -ForegroundColor Green
}
else {
    Write-Host "FAIL - Some carousel movies have multiple genres" -ForegroundColor Red
}
