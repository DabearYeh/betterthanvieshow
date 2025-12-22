# Debug login response
$baseUrl = "http://localhost:5041"

$loginBody = @{
    email    = "admin1234@gmail.com"
    password = "Admin1234"
} | ConvertTo-Json

Write-Host "Calling login API..."
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody

Write-Host "Full Response:"
$loginResponse | ConvertTo-Json -Depth 5

Write-Host "`nResponse properties:"
$loginResponse.PSObject.Properties | ForEach-Object { Write-Host "  $($_.Name): $($_.Value)" }
