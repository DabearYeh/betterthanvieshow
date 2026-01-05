# 測試 canDelete 欄位的腳本 (使用手動提供的 Token)

param(
    [Parameter(Mandatory = $true)]
    [string]$Token
)

Write-Host "===== Test GET /api/admin/theaters - canDelete field =====" -ForegroundColor Cyan

# 取得影廳列表
Write-Host "`nGetting theaters list..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $Token"
    }
    
    $theatersResponse = Invoke-RestMethod -Uri "http://localhost:5041/api/admin/theaters" `
        -Method Get `
        -Headers $headers

    Write-Host "Success! Retrieved theaters list" -ForegroundColor Green
    Write-Host "`nTheater Information:" -ForegroundColor Cyan
    
    foreach ($theater in $theatersResponse.data) {
        Write-Host "`n  Theater ID: $($theater.id)" -ForegroundColor White
        Write-Host "  Name: $($theater.name)" -ForegroundColor White
        Write-Host "  Type: $($theater.type)" -ForegroundColor White
        Write-Host "  Floor: $($theater.floor)" -ForegroundColor White
        Write-Host "  Seats: Standard=$($theater.standard), Wheelchair=$($theater.wheelchair)" -ForegroundColor White
        
        if ($theater.canDelete -eq $true) {
            Write-Host "  Can Delete: YES (No showtimes)" -ForegroundColor Green
        }
        else {
            Write-Host "  Can Delete: NO (Has showtimes)" -ForegroundColor Red
        }
    }

    Write-Host "`n===== Test Complete =====" -ForegroundColor Cyan
    Write-Host "Total theaters: $($theatersResponse.data.Count)" -ForegroundColor Cyan
    
    # 顯示完整的 JSON 回應
    Write-Host "`n===== Full JSON Response =====" -ForegroundColor Cyan
    $theatersResponse.data | ConvertTo-Json -Depth 5
    
}
catch {
    Write-Host "Failed to get theaters: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Error details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    exit 1
}
