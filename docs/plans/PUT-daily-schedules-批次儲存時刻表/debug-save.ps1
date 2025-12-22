# Debug test - capture detailed error
$baseUrl = "http://localhost:5041"

# Login
$loginBody = @{ email = "admin1234@gmail.com"; password = "Admin1234" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/login" -Method POST -ContentType "application/json" -Body $loginBody
$token = $loginResponse.data.token

$headers = @{ "Authorization" = "Bearer $token"; "Content-Type" = "application/json" }

# Test save with detailed error
$requestBody = @{
    showtimes = @(
        @{ movieId = 1; theaterId = 1; startTime = "09:45" }
    )
} | ConvertTo-Json -Depth 3

Write-Host "Request Body:"
Write-Host $requestBody

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-30" -Method PUT -Headers $headers -Body $requestBody
    Write-Host "SUCCESS!"
    $response | ConvertTo-Json -Depth 5
}
catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error Body:"
        Write-Host $errorBody
    }
}
