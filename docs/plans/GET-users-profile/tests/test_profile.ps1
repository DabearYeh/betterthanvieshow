$baseUrl = "http://localhost:5041"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$email = "testuser_$timestamp@example.com"
$password = "Password123!"

# 1. Register
$registerBody = @{
    name = "Test User"
    email = $email
    password = $password
} | ConvertTo-Json

Write-Host "1. Registering user $email..."
try {
    $regResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $registerBody -ContentType "application/json" -ErrorAction Stop
    Write-Host "   Registration successful." -ForegroundColor Green
} catch {
    Write-Host "   Registration failed: $_" -ForegroundColor Red
    exit
}

# 2. Extract Token
$token = $regResponse.data.token
if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Host "   Error: Token not found in response." -ForegroundColor Red
    exit
}
Write-Host "   Got Token: $($token.Substring(0, 10))..."

# 3. Get Profile
$headers = @{
    Authorization = "Bearer $token"
}

Write-Host "2. Fetching Profile..."
try {
    $profileResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/profile" -Method Get -Headers $headers -ErrorAction Stop
    
    Write-Host "   API Response:"
    Write-Host ($profileResponse | ConvertTo-Json -Depth 5)

    if ($profileResponse.data.email -eq $email) {
        Write-Host "3. VERIFICATION: SUCCESS - Email matches!" -ForegroundColor Green
    } else {
        Write-Host "3. VERIFICATION: FAILURE - Email mismatch! Expected $email, got $($profileResponse.data.email)" -ForegroundColor Red
    }
} catch {
    Write-Host "   Failed to get profile: $_" -ForegroundColor Red
}
