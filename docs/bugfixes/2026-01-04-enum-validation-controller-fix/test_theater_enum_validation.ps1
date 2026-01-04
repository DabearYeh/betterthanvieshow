# Theater API Enum 驗證測試腳本
$baseUrl = "http://localhost:5041"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW4xMjM0QGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiLnrqHnkIblk6EiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc2ODEzNjUyNCwiaXNzIjoiQmV0dGVyVGhhblZpZVNob3dBUEkiLCJhdWQiOiJCZXR0ZXJUaGFuVmllU2hvd0NsaWVudCJ9.DorOOVMfPfpqjcWeIpFSOFzr0iL4dvcT3-hg6udCpdg"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Theater API Enum 驗證測試" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# 測試 1: ✅ 正確的 Theater Type - Digital
Write-Host "[測試 1] ✅ 正確的 Type: Digital" -ForegroundColor Yellow
$body1 = @{
    name        = "測試數位廳-$(Get-Date -Format 'HHmmss')"
    type        = "Digital"
    floor       = 3
    rowCount    = 2
    columnCount = 3
    seats       = @(
        @("Standard", "Aisle", "Standard"),
        @("Wheelchair", "Aisle", "Standard")
    )
} | ConvertTo-Json -Depth 10

try {
    $response1 = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Post -Headers $headers -Body $body1
    Write-Host "  結果: SUCCESS ✓" -ForegroundColor Green
    Write-Host "  訊息: $($response1.message)" -ForegroundColor Green
}
catch {
    Write-Host "  結果: FAILED ✗" -ForegroundColor Red
    Write-Host "  錯誤: $($_.Exception.Message)" -ForegroundColor Red
}

# 測試 2: ❌ 錯誤的 Theater Type - 中文值
Write-Host "`n[測試 2] ❌ 錯誤的 Type: 一般數位 (應回傳 400)" -ForegroundColor Yellow
$body2 = @{
    name        = "測試錯誤類型廳"
    type        = "一般數位"
    floor       = 3
    rowCount    = 2
    columnCount = 3
    seats       = @(
        @("Standard", "Aisle", "Standard"),
        @("Standard", "Aisle", "Standard")
    )
} | ConvertTo-Json -Depth 10

try {
    $response2 = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Post -Headers $headers -Body $body2
    Write-Host "  結果: UNEXPECTED SUCCESS ✗ (應該要失敗)" -ForegroundColor Red
}
catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "  結果: CORRECT FAILURE ✓" -ForegroundColor Green
        Write-Host "  狀態碼: 400 Bad Request" -ForegroundColor Green
        Write-Host "  錯誤訊息: $($errorResponse.message)" -ForegroundColor Green
    }
    else {
        Write-Host "  結果: WRONG ERROR CODE ✗" -ForegroundColor Red
        Write-Host "  狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# 測試 3: ❌ 錯誤的 Seat Type - 中文值
Write-Host "`n[測試 3] ❌ 錯誤的 SeatType: 普通, 走道 (應回傳 400)" -ForegroundColor Yellow
$body3 = @{
    name        = "測試錯誤座位類型廳"
    type        = "Digital"
    floor       = 3
    rowCount    = 2
    columnCount = 3
    seats       = @(
        @("普通", "走道", "Standard"),
        @("Standard", "Aisle", "Standard")
    )
} | ConvertTo-Json -Depth 10

try {
    $response3 = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Post -Headers $headers -Body $body3
    Write-Host "  結果: UNEXPECTED SUCCESS ✗ (應該要失敗)" -ForegroundColor Red
}
catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "  結果: CORRECT FAILURE ✓" -ForegroundColor Green
        Write-Host "  狀態碼: 400 Bad Request" -ForegroundColor Green
        Write-Host "  錯誤訊息: $($errorResponse.message)" -ForegroundColor Green
    }
    else {
        Write-Host "  結果: WRONG ERROR CODE ✗" -ForegroundColor Red
        Write-Host "  狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# 測試 4: ❌ 錯誤的 Theater Type - 隨機值
Write-Host "`n[測試 4] ❌ 錯誤的 Type: InvalidType (應回傳 400)" -ForegroundColor Yellow
$body4 = @{
    name        = "測試錯誤類型廳2"
    type        = "InvalidType"
    floor       = 3
    rowCount    = 2
    columnCount = 3
    seats       = @(
        @("Standard", "Aisle", "Standard"),
        @("Standard", "Aisle", "Standard")
    )
} | ConvertTo-Json -Depth 10

try {
    $response4 = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Post -Headers $headers -Body $body4
    Write-Host "  結果: UNEXPECTED SUCCESS ✗ (應該要失敗)" -ForegroundColor Red
}
catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "  結果: CORRECT FAILURE ✓" -ForegroundColor Green
        Write-Host "  狀態碼: 400 Bad Request" -ForegroundColor Green
        Write-Host "  錯誤訊息: $($errorResponse.message)" -ForegroundColor Green
    }
    else {
        Write-Host "  結果: WRONG ERROR CODE ✗" -ForegroundColor Red
        Write-Host "  狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n======================================== " -ForegroundColor Cyan
Write-Host "Theater API 測試完成" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan
