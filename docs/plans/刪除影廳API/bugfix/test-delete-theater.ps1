# 測試刪除影廳 API
$baseUrl = "http://localhost:5041"

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "步驟 1: 登入取得 Token" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

# 登入取得 token
$loginBody = @{
    email    = "admin1234@gmail.com"
    password = "Admin123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "✓ 登入成功，Token 已取得" -ForegroundColor Green
}
catch {
    Write-Host "✗ 登入失敗: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

Write-Host "`n===========================================" -ForegroundColor Cyan
Write-Host "步驟 2: 查詢所有影廳" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

try {
    $theaters = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Get -Headers $headers
    Write-Host "目前影廳列表:" -ForegroundColor Yellow
    $theaters.data | ForEach-Object { 
        Write-Host "  - ID: $($_.id), 名稱: $($_.name), 類型: $($_.type), 總座位: $($_.totalSeats)" 
    }
}
catch {
    Write-Host "✗ 查詢影廳失敗: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n===========================================" -ForegroundColor Cyan
Write-Host "步驟 3: 測試刪除影廳 ID=2" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/admin/theaters/2" -Method Delete -Headers $headers -UseBasicParsing
    
    Write-Host "`n✓ 狀態碼: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "回應內容:" -ForegroundColor Green
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "`n✗ 狀態碼: $statusCode" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "錯誤回應內容:" -ForegroundColor Yellow
        try {
            $responseBody | ConvertFrom-Json | ConvertTo-Json -Depth 10
        }
        catch {
            Write-Host $responseBody
        }
    }
    else {
        Write-Host "錯誤訊息: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n===========================================" -ForegroundColor Cyan
Write-Host "步驟 4: 再次查詢所有影廳（驗證刪除結果）" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

try {
    $theaters = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Get -Headers $headers
    Write-Host "刪除後的影廳列表:" -ForegroundColor Yellow
    $theaters.data | ForEach-Object { 
        Write-Host "  - ID: $($_.id), 名稱: $($_.name), 類型: $($_.type), 總座位: $($_.totalSeats)" 
    }
}
catch {
    Write-Host "✗ 查詢影廳失敗: $($_.Exception.Message)" -ForegroundColor Red
}
