# IIS 網站配置腳本
# 此腳本用於在 Azure VM 上初始化 IIS 網站配置

param(
    [string]$SiteName = "BetterThanVieShow",
    [string]$SitePath = "C:\inetpub\wwwroot\betterthanvieshow",
    [string]$AppPoolName = "BetterThanVieShowAppPool",
    [int]$Port = 80,
    [int]$HttpsPort = 443
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "IIS 網站配置腳本" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查是否以管理員身份執行
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "❌ 錯誤: 請以管理員身份執行此腳本" -ForegroundColor Red
    exit 1
}

# 步驟 1: 安裝 IIS 功能
Write-Host "📦 步驟 1/5: 檢查 IIS 功能..." -ForegroundColor Yellow
try {
    $iisFeature = Get-WindowsFeature -Name Web-Server
    if (-not $iisFeature.Installed) {
        Write-Host "  正在安裝 IIS..." -ForegroundColor Gray
        Install-WindowsFeature -Name Web-Server -IncludeManagementTools
        Install-WindowsFeature -Name Web-Asp-Net45
        Write-Host "✓ IIS 已安裝" -ForegroundColor Green
    }
    else {
        Write-Host "✓ IIS 已安裝" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠ 無法檢查 IIS 狀態: $_" -ForegroundColor Yellow
}

# 步驟 2: 建立應用程式池
Write-Host ""
Write-Host "🔧 步驟 2/5: 配置應用程式池..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration
    
    # 檢查應用程式池是否存在
    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        Write-Host "  應用程式池已存在，正在移除..." -ForegroundColor Gray
        Remove-WebAppPool -Name $AppPoolName
    }
    
    # 建立新的應用程式池
    New-WebAppPool -Name $AppPoolName
    
    # 設定應用程式池屬性
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "enable32BitAppOnWin64" -Value $false
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"
    
    Write-Host "✓ 應用程式池已配置: $AppPoolName" -ForegroundColor Green
}
catch {
    Write-Host "❌ 配置應用程式池失敗: $_" -ForegroundColor Red
    exit 1
}

# 步驟 3: 建立網站目錄
Write-Host ""
Write-Host "📁 步驟 3/5: 建立網站目錄..." -ForegroundColor Yellow
try {
    if (-not (Test-Path $SitePath)) {
        New-Item -ItemType Directory -Path $SitePath -Force | Out-Null
        Write-Host "✓ 目錄已建立: $SitePath" -ForegroundColor Green
    }
    else {
        Write-Host "✓ 目錄已存在: $SitePath" -ForegroundColor Green
    }
    
    # 設定權限
    $acl = Get-Acl $SitePath
    $permission = "IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    Set-Acl $SitePath $acl
    
    Write-Host "✓ 權限已設定" -ForegroundColor Green
}
catch {
    Write-Host "⚠ 設定目錄權限時發生警告: $_" -ForegroundColor Yellow
}

# 步驟 4: 建立或更新網站
Write-Host ""
Write-Host "🌐 步驟 4/5: 配置 IIS 網站..." -ForegroundColor Yellow
try {
    # 檢查網站是否存在
    if (Test-Path "IIS:\Sites\$SiteName") {
        Write-Host "  網站已存在，正在移除..." -ForegroundColor Gray
        Remove-Website -Name $SiteName
    }
    
    # 建立新網站
    New-Website -Name $SiteName `
        -PhysicalPath $SitePath `
        -ApplicationPool $AppPoolName `
        -Port $Port
    
    Write-Host "✓ 網站已建立: $SiteName" -ForegroundColor Green
    Write-Host "  繫結: http://*:$Port" -ForegroundColor Gray
}
catch {
    Write-Host "❌ 建立網站失敗: $_" -ForegroundColor Red
    exit 1
}

# 步驟 5: 建立 web.config
Write-Host ""
Write-Host "📄 步驟 5/5: 建立 web.config..." -ForegroundColor Yellow
$webConfigPath = Join-Path $SitePath "web.config"
$webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\betterthanvieshow.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

try {
    Set-Content -Path $webConfigPath -Value $webConfigContent -Force
    Write-Host "✓ web.config 已建立" -ForegroundColor Green
}
catch {
    Write-Host "⚠ 建立 web.config 失敗: $_" -ForegroundColor Yellow
}

# 建立日誌目錄
$logsPath = Join-Path $SitePath "logs"
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "✓ IIS 配置完成！" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "配置資訊:" -ForegroundColor White
Write-Host "  網站名稱: $SiteName" -ForegroundColor Gray
Write-Host "  應用程式池: $AppPoolName" -ForegroundColor Gray
Write-Host "  網站路徑: $SitePath" -ForegroundColor Gray
Write-Host "  HTTP 埠: $Port" -ForegroundColor Gray
Write-Host ""
Write-Host "下一步:" -ForegroundColor Yellow
Write-Host "  1. 確保已安裝 .NET 9.0 Hosting Bundle" -ForegroundColor Gray
Write-Host "  2. 確保已安裝 ASP.NET Core Module v2" -ForegroundColor Gray
Write-Host "  3. 部署應用程式到: $SitePath" -ForegroundColor Gray
Write-Host "  4. 在瀏覽器中訪問: http://localhost:$Port" -ForegroundColor Gray
Write-Host ""
