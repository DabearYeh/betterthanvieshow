# CI/CD 快速啟動指南

本指南提供快速啟動 CI/CD 流程的核心步驟。

---

## 📋 前置檢查清單

在開始之前，請確認您已準備：

- [ ] Azure VM 訪問權限
- [ ] GitHub Repository 管理員權限
- [ ] Azure SQL Database 連線字串
- [ ] JWT Secret Key (至少 32 字元)

---

## 🚀 快速設置步驟

### 1️⃣ 在 Azure VM 上安裝必要軟體

連接到 Azure VM，然後執行：

```powershell
# 1. 安裝 .NET 9.0 Hosting Bundle
# 從 https://dotnet.microsoft.com/download/dotnet/9.0 下載並安裝

# 2. 重啟伺服器
Restart-Computer -Force

# 3. 驗證安裝
dotnet --info
```

### 2️⃣ 配置 IIS

```powershell
# 從您的專案複製 setup-iis.ps1 到 VM，然後執行：
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
.\setup-iis.ps1
```

### 3️⃣ 安裝 GitHub Actions Runner

```powershell
# 1. 建立目錄
mkdir C:\actions-runner
cd C:\actions-runner

# 2. 前往 GitHub 取得 Runner Token:
# https://github.com/YOUR_USERNAME/betterthanvieshow/settings/actions/runners/new

# 3. 下載並配置 Runner（使用 GitHub 頁面上的指令）

# 4. 安裝為服務
.\svc.sh install
.\svc.sh start
```

### 4️⃣ 設定 GitHub Secrets

前往 GitHub Repository **Settings** > **Secrets and variables** > **Actions**，添加：

| Secret 名稱 | 值 |
|------------|-----|
| `AZURE_SQL_CONNECTION_STRING` | 您的 Azure SQL 連線字串 |
| `JWT_SECRET_KEY` | 您的 JWT 密鑰 (32+ 字元) |
| `IIS_SITE_PATH` | `C:\inetpub\wwwroot\betterthanvieshow` |
| `IIS_APP_POOL_NAME` | `BetterThanVieShowAppPool` |
| `SITE_URL` | 您的網站 URL |

### 5️⃣ 推送程式碼觸發 CI/CD

```bash
# 1. 確保所有新檔案都已提交
git add .
git commit -m "feat: add CI/CD pipeline"
git push origin main

# 2. 前往 GitHub Actions 查看執行狀態
```

---

## ✅ 驗證部署

### 檢查 Runner 狀態
- 前往: https://github.com/YOUR_USERNAME/betterthanvieshow/settings/actions/runners
- 確認 Runner 顯示綠色 "Idle" 狀態

### 檢查部署結果
```bash
# 訪問健康檢查端點
curl http://your-server/health

# 應該返回: Healthy
```

---

## 🔄 日常使用流程

### 自動部署 (推薦)

```bash
# 1. 在本地開發並測試
git checkout -b feature/your-feature
# ... 開發 ...
git add .
git commit -m "feat: your changes"
git push origin feature/your-feature

# 2. 建立 Pull Request 到 main

# 3. 合併 PR 後自動部署到生產環境
```

### 手動部署 (緊急使用)

在 Azure VM 上：

```powershell
cd C:\path\to\betterthanvieshow
.\scripts\deploy.ps1 -ConnectionString "YOUR_CONNECTION_STRING"
```

---

## 📊 監控與日誌

### GitHub Actions 日誌
- https://github.com/YOUR_USERNAME/betterthanvieshow/actions

### IIS 應用程式日誌
```powershell
# 在 Azure VM 上檢查日誌
Get-Content "C:\inetpub\wwwroot\betterthanvieshow\logs\stdout*.log" -Tail 50
```

### Windows 事件日誌
```powershell
Get-EventLog -LogName Application -Source "IIS*" -Newest 20
```

---

## 🆘 常見問題

### Runner 顯示離線？

```powershell
# 在 Azure VM 上重啟 Runner 服務
cd C:\actions-runner
.\svc.sh stop
.\svc.sh start
```

### 部署失敗？

1. 檢查 GitHub Actions 日誌查看錯誤訊息
2. 確認所有 Secrets 都已正確設定
3. 確認 Azure VM 上的 Runner 正在運行
4. 檢查 IIS 應用程式池狀態

```powershell
Get-WebAppPoolState -Name "BetterThanVieShowAppPool"
```

### 網站無法訪問？

1. 檢查防火牆規則
2. 檢查 Azure NSG 設定
3. 確認 IIS 網站已啟動

```powershell
Get-Website -Name "BetterThanVieShow"
```

---

**🎉 設置完成！每次推送到 main 分支都會自動部署！**
