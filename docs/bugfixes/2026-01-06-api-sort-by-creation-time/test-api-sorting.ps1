# 完整測試腳本 - API 排序驗證
# 用途：完整測試影廳和電影 API 的排序功能，包含詳細的輸出和驗證

param(
    [string]$BaseUrl = "http://localhost:5041",
    [string]$Token = ""
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  API 排序功能完整測試" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# 檢查 Token
if ([string]::IsNullOrWhiteSpace($Token)) {
    Write-Host "請提供 JWT Token（使用 -Token 參數或互動輸入）" -ForegroundColor Yellow
    $Token = Read-Host "JWT Token"
    
    if ([string]::IsNullOrWhiteSpace($Token)) {
        Write-Host "❌ 錯誤: Token 不能為空" -ForegroundColor Red
        exit 1
    }
}

$headers = @{ 
    "Authorization" = "Bearer $Token"
    "Content-Type"  = "application/json"
}

# 測試結果統計
$totalTests = 0
$passedTests = 0
$failedTests = 0

function Test-ApiEndpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$SortField
    )
    
    $script:totalTests++
    
    Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host "測試: $Name" -ForegroundColor Cyan
    Write-Host "端點: $Url" -ForegroundColor Gray
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    
    try {
        # 發送請求
        $response = Invoke-RestMethod -Uri $Url -Method Get -Headers $headers
        
        if (-not $response.success) {
            Write-Host "❌ API 回應失敗: $($response.message)" -ForegroundColor Red
            $script:failedTests++
            return
        }
        
        $data = $response.data
        $count = $data.Count
        
        Write-Host "✅ 成功取得資料，共 $count 筆" -ForegroundColor Green
        Write-Host ""
        
        # 顯示所有資料
        Write-Host "完整列表（依順序）:" -ForegroundColor Yellow
        $data | ForEach-Object {
            $id = $_.id
            $name = if ($_.name) { $_.name } else { $_.title }
            Write-Host "  ID: $id - $name" -ForegroundColor White
        }
        
        # 驗證排序
        Write-Host ""
        Write-Host "排序驗證:" -ForegroundColor Yellow
        
        $ids = $data | ForEach-Object { $_.id }
        
        if ($ids.Count -eq 0) {
            Write-Host "  ⚠️  沒有資料可驗證" -ForegroundColor Yellow
            return
        }
        
        # 檢查是否降序
        $isDescending = $true
        for ($i = 0; $i -lt ($ids.Count - 1); $i++) {
            if ($ids[$i] -lt $ids[$i + 1]) {
                $isDescending = $false
                Write-Host "  ❌ 發現順序錯誤: ID $($ids[$i]) 後面是 ID $($ids[$i + 1])" -ForegroundColor Red
                break
            }
        }
        
        if ($isDescending) {
            Write-Host "  ✅ 排序正確：ID 按降序排列（新的在前）" -ForegroundColor Green
            Write-Host "  📊 最大 ID: $($ids[0])，最小 ID: $($ids[-1])" -ForegroundColor Cyan
            $script:passedTests++
        }
        else {
            Write-Host "  ❌ 排序錯誤：ID 沒有按降序排列" -ForegroundColor Red
            $script:failedTests++
        }
        
    }
    catch {
        Write-Host "❌ 測試失敗: $($_.Exception.Message)" -ForegroundColor Red
        $script:failedTests++
    }
}

# 執行測試
Write-Host "開始測試..." -ForegroundColor Green
Write-Host ""

# 測試影廳 API
Test-ApiEndpoint `
    -Name "影廳列表 API (GET /api/admin/theaters)" `
    -Url "$BaseUrl/api/admin/theaters" `
    -SortField "id"

# 測試電影 API
Test-ApiEndpoint `
    -Name "電影列表 API (GET /api/admin/movies)" `
    -Url "$BaseUrl/api/admin/movies" `
    -SortField "createdAt"

# 顯示測試摘要
Write-Host "`n" -ForegroundColor White
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  測試摘要" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "總測試數: $totalTests" -ForegroundColor White
Write-Host "通過: $passedTests" -ForegroundColor Green
Write-Host "失敗: $failedTests" -ForegroundColor $(if ($failedTests -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($failedTests -eq 0) {
    Write-Host "🎉 所有測試通過！" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "⚠️  有測試失敗，請檢查實作" -ForegroundColor Red
    exit 1
}
