# Self-Hosted Runner 安裝與設置指南

## 🎯 為什麼選擇 Self-Hosted Runner？

### 優勢
- ✅ **最高安全性**：不需要開放任何 FTP/WinRM 端口給外網
- ✅ **最快速度**：檔案複製都在 VM 內部進行，沒有網路傳輸延遲
- ✅ **完全控制**：可以執行任何 PowerShell 指令，包括重啟 IIS
- ✅ **簡單設置**：不需要複雜的防火牆配置
- ✅ **免費**：Self-Hosted Runner 完全免費使用

### 工作原理
```
GitHub ← (定時詢問) ← Runner (在 Azure VM 內) → 執行部署 → IIS
```

Runner 像一個「住在 VM 裡的管家」，定時問 GitHub：「有工作給我嗎？」

---

## 📋 安裝步驟

### Step 1: 在 GitHub 上設置 Runner

1. **前往 GitHub Repository**
   - 打開您的專案：`https://github.com/您的用戶名/betterthanvieshow`

2. **進入 Settings → Actions → Runners**
   - 點擊上方的 **Settings** 標籤
   - 左側選單選擇 **Actions** → **Runners**
   - 點擊右上角的 **New self-hosted runner** 按鈕

3. **選擇作業系統和架構**
   - Runner image: **Windows**
   - Architecture: **x64**

4. **複製顯示的指令**（稍後會用到）

---

### Step 2: 在 Azure VM 上安裝 Runner

**登入您的 Azure Windows Server VM**，然後執行以下步驟：

#### 2.1 創建 Runner 工作目錄

```powershell
# 以管理員身份開啟 PowerShell

# 創建 Runner 目錄
New-Item -ItemType Directory -Path "C:\actions-runner" -Force
cd C:\actions-runner
```

#### 2.2 下載 Runner 程式

```powershell
# 下載最新版本的 GitHub Actions Runner
# 注意：請使用 GitHub 上顯示的最新下載連結
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/v2.311.0/actions-runner-win-x64-2.311.0.zip -OutFile actions-runner-win-x64-2.311.0.zip

# 解壓縮
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory("$PWD\actions-runner-win-x64-2.311.0.zip", "$PWD")
```

> **提示**：實際下載連結請使用 GitHub Settings → Runners 頁面中顯示的連結，版本號可能不同。

#### 2.3 配置 Runner

```powershell
# 執行配置腳本
# 替換下面的 URL 和 TOKEN 為 GitHub 頁面上顯示的實際值
.\config.cmd --url https://github.com/您的用戶名/betterthanvieshow --token YOUR_GITHUB_TOKEN

# 配置過程中的選項：
# - Enter the name of the runner group: [直接按 Enter，使用預設值]
# - Enter the name of runner: [輸入 azure-vm-runner 或其他名稱]
# - Enter any additional labels: [直接按 Enter]
# - Enter name of work folder: [直接按 Enter，使用預設 _work]
```

#### 2.4 測試 Runner（可選）

```powershell
# 啟動 Runner（測試用）
.\run.cmd

# 您應該會看到：
# √ Connected to GitHub
# 當前時間......等待工作...
```

測試成功後，按 `Ctrl+C` 停止。

#### 2.5 安裝為 Windows 服務（重要！）

```powershell
# 將 Runner 安裝為 Windows 服務，這樣 VM 重啟後會自動啟動
.\svc.cmd install

# 啟動服務
.\svc.cmd start

# 檢查服務狀態
.\svc.cmd status
# 應該顯示：Active: active (running)
```

---

### Step 3: 在 GitHub 設置 Secrets

前往 GitHub Repository → **Settings** → **Secrets and variables** → **Actions**

添加以下 Secrets：

| Secret Name | Value 範例 | 說明 |
|------------|-----------|------|
| `IIS_SITE_PATH` | `C:\inetpub\wwwroot\betterthanvieshow` | IIS 網站的實體路徑 |
| `IIS_APP_POOL_NAME` | `betterthanvieshow` | IIS Application Pool 的名稱 |

#### 如何找到這些值？

**找到 IIS_SITE_PATH：**
```powershell
# 在 Azure VM 上執行
Get-Website | Select-Object Name, PhysicalPath

# 輸出範例：
# Name              PhysicalPath
# ----              ------------
# betterthanvieshow C:\inetpub\wwwroot\betterthanvieshow
```

**找到 IIS_APP_POOL_NAME：**
```powershell
# 通常和網站名稱相同
Get-WebAppPoolState *

# 或在 IIS Manager 中查看網站設定
```

---

### Step 4: 準備 IIS 網站

如果還沒有創建 IIS 網站，請執行：

```powershell
# 1. 創建網站目錄
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\betterthanvieshow" -Force

# 2. 創建 Application Pool
New-WebAppPool -Name "betterthanvieshow"
Set-ItemProperty IIS:\AppPools\betterthanvieshow -Name managedRuntimeVersion -Value ""

# 3. 創建網站
New-Website -Name "betterthanvieshow" `
           -PhysicalPath "C:\inetpub\wwwroot\betterthanvieshow" `
           -ApplicationPool "betterthanvieshow" `
           -Port 80

# 4. 啟動網站
Start-Website -Name "betterthanvieshow"
```

---

## ✅ 驗證設置

### 1. 檢查 Runner 狀態

**在 GitHub 上：**
- Settings → Actions → Runners
- 應該看到您的 Runner 顯示為 **綠色的 "Idle"** 狀態

**在 Azure VM 上：**
```powershell
cd C:\actions-runner
.\svc.cmd status

# 應該顯示：
# Active: active (running)
```

### 2. 測試完整流程

```bash
# 在本地推送程式碼
git add .
git commit -m "test: verify self-hosted runner deployment"
git push origin main
```

**預期結果：**
1. CI workflow 執行（在 GitHub 提供的 runner 上）
2. CD workflow 執行（在您的 Azure VM 上）
3. 檔案自動部署到 IIS
4. Application Pool 自動重啟
5. 網站更新完成

---

## 🔧 故障排除

### Runner 離線？

```powershell
# 重啟 Runner 服務
cd C:\actions-runner
.\svc.cmd stop
.\svc.cmd start
.\svc.cmd status
```

### 權限問題？

```powershell
# 確保 Runner 服務使用的帳戶有足夠權限
# 預設是 NETWORK SERVICE，需要對網站目錄有寫入權限

icacls "C:\inetpub\wwwroot\betterthanvieshow" /grant "NETWORK SERVICE":(OI)(CI)F
```

### 查看 Runner 日誌

```powershell
# Runner 日誌位置
Get-Content "C:\actions-runner\_diag\Runner_*.log" -Tail 50
```

---

## 💡 進階配置（可選）

### 配置環境變數

如果需要在部署時使用環境變數：

```powershell
# 在 VM 上設置系統環境變數
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging", "Machine")
```

### appsettings.json 保護

CD workflow 已設置為 `-Force` 覆蓋，如果您想保留特定配置：

**方法 1：使用環境變數（推薦）**
- 在 Azure VM 上設置環境變數
- 不在 appsettings.json 中存放敏感資料

**方法 2：排除特定檔案**

修改 CD workflow 的複製步驟：
```powershell
# 排除 appsettings.json
Get-ChildItem "${{ github.workspace }}/publish/*" -Exclude "appsettings.json" | 
  Copy-Item -Destination "${{ secrets.IIS_SITE_PATH }}" -Recurse -Force
```

---

## 🎉 完成！

現在您擁有：
- ✅ 完全自動化的 CI/CD pipeline
- ✅ 最安全的部署方式（不開放任何端口）
- ✅ 最快的部署速度（直接在 VM 內部）
- ✅ 自動備份機制（保留最近 5 個版本）
- ✅ 自動重啟 IIS

每次 push 到 main 分支，幾分鐘內您的 Demo 環境就會自動更新！

---

## 📞 需要幫助？

如果在安裝過程中遇到任何問題，請告訴我具體的錯誤訊息，我會協助您排查！
