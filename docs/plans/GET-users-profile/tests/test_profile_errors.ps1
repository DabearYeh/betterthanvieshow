$baseUrl = "http://localhost:5041"

# --- TestCase 1: 401 Unauthorized (No Token) ---
Write-Host "`n--- Testing 401 Unauthorized (No Token) ---"
try {
    Invoke-RestMethod -Uri "$baseUrl/api/users/profile" -Method Get -ErrorAction Stop
    Write-Host "FAILURE: API allowed access without token!" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode
    if ([int]$statusCode -eq 401) {
        Write-Host "SUCCESS: API returned 401 Unauthorized as expected." -ForegroundColor Green
    }
    else {
        Write-Host "FAILURE: Expected 401, but got $statusCode" -ForegroundColor Red
    }
}

# --- TestCase 2: 401 Unauthorized (Invalid Token) ---
Write-Host "`n--- Testing 401 Unauthorized (Invalid Token) ---"
$invalidHeaders = @{ Authorization = "Bearer invalid_token_123" }
try {
    Invoke-RestMethod -Uri "$baseUrl/api/users/profile" -Method Get -Headers $invalidHeaders -ErrorAction Stop
    Write-Host "FAILURE: API allowed access with invalid token!" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode
    if ([int]$statusCode -eq 401) {
        Write-Host "SUCCESS: API returned 401 Unauthorized for invalid token." -ForegroundColor Green
    }
    else {
        Write-Host "FAILURE: Expected 401, but got $statusCode" -ForegroundColor Red
    }
}
