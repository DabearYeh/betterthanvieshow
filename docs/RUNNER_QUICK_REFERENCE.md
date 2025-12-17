# Self-Hosted Runner 快速參考

## 🚀 快速設置（3 步驟）

### 1️⃣ 在 GitHub 上添加 Runner
```
Settings → Actions → Runners → New self-hosted runner
選擇: Windows x64
複製顯示的指令
```

### 2️⃣ 在 Azure VM 上安裝
```powershell
# 創建目錄
New-Item -ItemType Directory -Path "C:\actions-runner" -Force
cd C:\actions-runner

# 下載並解壓（使用 GitHub 顯示的連結）
# 下載網址範例（請使用最新版本）：
# https://github.com/actions/runner/releases/download/v2.311.0/actions-runner-win-x64-2.311.0.zip

# 配置（替換為 GitHub 上的實際 TOKEN）
.\config.cmd --url https://github.com/您的用戶名/betterthanvieshow --token YOUR_TOKEN

# 安裝為服務
.\svc.cmd install
.\svc.cmd start
```

### 3️⃣ 設置 GitHub Secrets
```
Settings → Secrets and variables → Actions → New repository secret

添加:
- IIS_SITE_PATH = C:\inetpub\wwwroot\betterthanvieshow
- IIS_APP_POOL_NAME = betterthanvieshow
```

## ✅ 驗證

### GitHub 上
```
Settings → Runners → 應該顯示綠色 "Idle"
```

### Azure VM 上
```powershell
cd C:\actions-runner
.\svc.cmd status
# 應該顯示: Active: active (running)
```

## 🔧 常用指令

### 管理 Runner 服務
```powershell
cd C:\actions-runner

# 查看狀態
.\svc.cmd status

# 停止服務
.\svc.cmd stop

# 啟動服務
.\svc.cmd start

# 重啟服務
.\svc.cmd stop
.\svc.cmd start

# 解除安裝服務
.\svc.cmd uninstall
```

### 查看日誌
```powershell
# Runner 日誌
Get-Content "C:\actions-runner\_diag\Runner_*.log" -Tail 50

# Worker 日誌（實際執行的工作）
Get-Content "C:\actions-runner\_diag\Worker_*.log" -Tail 50
```

### 檢查 IIS 狀態
```powershell
# 查看所有網站
Get-Website

# 查看 Application Pool 狀態
Get-WebAppPoolState *

# 手動重啟 Application Pool
Restart-WebAppPool -Name "betterthanvieshow"

# 查看網站路徑
Get-Website | Select-Object Name, PhysicalPath
```

## 🎯 部署流程

```
本地開發 → git push origin main
    ↓
GitHub Actions CI (自動 build)
    ↓
CI 成功 → 觸發 CD workflow
    ↓
Self-Hosted Runner (在 Azure VM) 執行:
1. 下載 build 產物
2. 停止 IIS App Pool
3. 備份當前版本
4. 複製檔案
5. 啟動 IIS App Pool
    ↓
部署完成！Demo 環境更新
```

## ⚠️ 故障排除

### Runner 顯示離線
```powershell
cd C:\actions-runner
.\svc.cmd stop
.\svc.cmd start

# 檢查 Windows 服務
Get-Service | Where-Object {$_.Name -like "*actions*"}
```

### 權限錯誤
```powershell
# 給 NETWORK SERVICE 權限
icacls "C:\inetpub\wwwroot\betterthanvieshow" /grant "NETWORK SERVICE":(OI)(CI)F
```

### 部署後網站不更新
```powershell
# 手動重啟 Application Pool
Restart-WebAppPool -Name "betterthanvieshow"

# 或重啟 IIS
iisreset
```

### 查看詳細錯誤
```
GitHub → Actions → 點擊失敗的 workflow → 查看日誌
```

## 📞 需要幫助？

完整文檔：[SELF_HOSTED_RUNNER_SETUP.md](SELF_HOSTED_RUNNER_SETUP.md)

---

**最後更新**: 2025-12-17
