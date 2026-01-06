# 快速測試腳本 - API 排序驗證
# 用途：快速驗證影廳和電影 API 的排序是否正確

Write-Host "=== API 排序快速測試 ===" -ForegroundColor Cyan
Write-Host ""

# JWT Token (請替換成您的 token)
$token = Read-Host "請輸入您的 JWT Token"

if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Host "錯誤: Token 不能為空" -ForegroundColor Red
    exit 1
}

$headers = @{ 
    "Authorization" = "Bearer $token" 
}

$baseUrl = "http://localhost:5041"

# 測試影廳 API
Write-Host "`n1️⃣  測試影廳 API 排序..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/theaters" -Method Get -Headers $headers
    
    Write-Host "   ✅ 影廳總數: $($response.data.Count)" -ForegroundColor Green
    Write-Host "   前3個影廳（應該是 ID 降序）:" -ForegroundColor Cyan
    
    $response.data | Select-Object -First 3 | ForEach-Object {
        Write-Host "      ID: $($_.id) - $($_.name)" -ForegroundColor White
    }
    
    # 驗證排序
    $ids = $response.data | ForEach-Object { $_.id }
    $isSorted = ($ids | Measure-Object -Maximum).Maximum -eq $ids[0]
    
    if ($isSorted) {
        Write-Host "   ✅ 排序正確：新的在前面" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ 排序錯誤：請檢查實作" -ForegroundColor Red
    }
    
}
catch {
    Write-Host "   ❌ 測試失敗: $($_.Exception.Message)" -ForegroundColor Red
}

# 測試電影 API
Write-Host "`n2️⃣  測試電影 API 排序..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/movies" -Method Get -Headers $headers
    
    Write-Host "   ✅ 電影總數: $($response.data.Count)" -ForegroundColor Green
    Write-Host "   前3部電影（應該是 CreatedAt 降序）:" -ForegroundColor Cyan
    
    $response.data | Select-Object -First 3 | ForEach-Object {
        Write-Host "      ID: $($_.id) - $($_.title)" -ForegroundColor White
    }
    
    # 驗證排序
    $ids = $response.data | ForEach-Object { $_.id }
    $isSorted = ($ids | Measure-Object -Maximum).Maximum -eq $ids[0]
    
    if ($isSorted) {
        Write-Host "   ✅ 排序正確：新的在前面" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ 排序錯誤：請檢查實作" -ForegroundColor Red
    }
    
}
catch {
    Write-Host "   ❌ 測試失敗: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== 測試完成 ===" -ForegroundColor Cyan
