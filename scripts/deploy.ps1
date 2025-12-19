# 部署腳本 - Windows Server + IIS
# 此腳本用於手動部署或故障排除

param(
    [string]$PublishPath = ".\publish",
    [string]$SitePath = "C:\inetpub\wwwroot\betterthanvieshow",
    [string]$AppPoolName = "BetterThanVieShowAppPool",
    [string]$ConnectionString = ""
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "BetterThanVieShow 部署腳本" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查是否以管理員身份執行
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "❌ 錯誤: 請以管理員身份執行此腳本" -ForegroundColor Red
    exit 1
}

# 步驟 1: 建置專案
Write-Host "📦 步驟 1/7: 建置專案..." -ForegroundColor Yellow
try {
    Set-Location ".\betterthanvieshow"
    dotnet restore
    dotnet build --configuration Release
    dotnet publish --configuration Release --output ..\publish
    Set-Location ..
    Write-Host "✓ 建置完成" -ForegroundColor Green
} catch {
    Write-Host "❌ 建置失敗: $_" -ForegroundColor Red
    exit 1
}

# 步驟 2: 停止 IIS 應用程式池
Write-Host ""
Write-Host "⏸️  步驟 2/7: 停止 IIS 應用程式池..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration
    if (Get-WebAppPoolState -Name $AppPoolName | Where-Object { $_.Value -eq "Started" }) {
        Stop-WebAppPool -Name $AppPoolName
        Start-Sleep -Seconds 5
        Write-Host "✓ 應用程式池已停止" -ForegroundColor Green
    } else {
        Write-Host "⚠ 應用程式池已經是停止狀態" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ 無法停止應用程式池: $_" -ForegroundColor Yellow
}

# 步驟 3: 備份現有版本
Write-Host ""
Write-Host "💾 步驟 3/7: 備份現有版本..." -ForegroundColor Yellow
try {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupPath = "${SitePath}_backup_$timestamp"
    
    if (Test-Path $SitePath) {
        Copy-Item -Path $SitePath -Destination $backupPath -Recurse -Force
        Write-Host "✓ 備份完成: $backupPath" -ForegroundColor Green
    } else {
        Write-Host "⚠ 目標路徑不存在，跳過備份" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ 備份失敗: $_" -ForegroundColor Yellow
}

# 步驟 4: 部署新版本
Write-Host ""
Write-Host "🚀 步驟 4/7: 部署新版本..." -ForegroundColor Yellow
try {
    # 確保目標目錄存在
    if (-not (Test-Path $SitePath)) {
        New-Item -ItemType Directory -Path $SitePath -Force | Out-Null
    }
    
    # 保存 web.config（如果存在）
    $webConfigPath = Join-Path $SitePath "web.config"
    $tempWebConfig = $null
    if (Test-Path $webConfigPath) {
        $tempWebConfig = Get-Content $webConfigPath -Raw
    }
    
    # 清理舊檔案
    Get-ChildItem -Path $SitePath -Exclude "web.config" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
    
    # 複製新檔案
    Copy-Item -Path "$PublishPath\*" -Destination $SitePath -Recurse -Force
    
    # 恢復 web.config（如果有的話）
    if ($tempWebConfig) {
        Set-Content -Path $webConfigPath -Value $tempWebConfig
    }
    
    Write-Host "✓ 部署完成" -ForegroundColor Green
} catch {
    Write-Host "❌ 部署失敗: $_" -ForegroundColor Red
    exit 1
}

# 步驟 5: 執行資料庫遷移
Write-Host ""
Write-Host "🗄️  步驟 5/7: 執行資料庫遷移..." -ForegroundColor Yellow
if ($ConnectionString) {
    try {
        Set-Location ".\betterthanvieshow"
        $env:ASPNETCORE_ENVIRONMENT = "Production"
        dotnet ef database update --connection $ConnectionString
        Set-Location ..
        Write-Host "✓ 資料庫遷移完成" -ForegroundColor Green
    } catch {
        Write-Host "⚠ 資料庫遷移失敗: $_" -ForegroundColor Yellow
        Write-Host "  請手動執行遷移" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ 未提供連線字串，跳過資料庫遷移" -ForegroundColor Yellow
}

# 步驟 6: 啟動 IIS 應用程式池
Write-Host ""
Write-Host "▶️  步驟 6/7: 啟動 IIS 應用程式池..." -ForegroundColor Yellow
try {
    Start-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 5
    Write-Host "✓ 應用程式池已啟動" -ForegroundColor Green
} catch {
    Write-Host "⚠ 無法啟動應用程式池: $_" -ForegroundColor Yellow
}

# 步驟 7: 驗證部署
Write-Host ""
Write-Host "🔍 步驟 7/7: 驗證部署..." -ForegroundColor Yellow
Write-Host "  正在等待應用程式啟動..." -ForegroundColor Gray
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "✓ 部署完成！" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "部署路徑: $SitePath" -ForegroundColor White
Write-Host "應用程式池: $AppPoolName" -ForegroundColor White
Write-Host "時間: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor White
Write-Host ""
Write-Host "建議檢查項目:" -ForegroundColor Yellow
Write-Host "  1. 檢查 IIS 應用程式池狀態" -ForegroundColor Gray
Write-Host "  2. 瀏覽網站確認正常運作" -ForegroundColor Gray
Write-Host "  3. 檢查應用程式日誌" -ForegroundColor Gray
Write-Host ""
