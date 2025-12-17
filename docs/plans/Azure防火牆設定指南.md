# Azure SQL Database 防火牆設定指南

## 問題說明

資料庫遷移失敗，錯誤訊息顯示無法連線到 Azure SQL Database。這是因為 Azure SQL Server 的防火牆預設會阻擋所有外部連線。

**您的本機 IP**: `36.238.11.186`

---

## 設定步驟

### 方法 1: 使用 Azure Portal（推薦）

1. **登入 Azure Portal**
   - 前往 https://portal.azure.com

2. **找到您的 SQL Server**
   - 在搜尋列輸入 `betterthanvieshow-sql`
   - 或從「所有資源」中找到 SQL Server

3. **進入防火牆設定**
   - 在左側選單點選「安全性」→「網路」
   - 或直接點選「防火牆與虛擬網路」

4. **新增防火牆規則**
   - 點選「+ 新增用戶端 IPv4」或「+ 新增防火牆規則」
   - 填寫以下資訊：
     ```
     規則名稱: MyLocalIP
     起始 IP: 36.238.11.186
     結束 IP: 36.238.11.186
     ```

5. **儲存設定**
   - 點選「儲存」按鈕
   - 等待幾秒讓設定生效

---

### 方法 2: 使用 Azure CLI（快速）

如果您已安裝 Azure CLI，可以執行以下命令：

```powershell
# 登入 Azure
az login

# 新增防火牆規則
az sql server firewall-rule create `
  --resource-group 您的資源群組名稱 `
  --server betterthanvieshow-sql `
  --name MyLocalIP `
  --start-ip-address 36.238.11.186 `
  --end-ip-address 36.238.11.186
```

---

### 方法 3: 允許 Azure 服務存取（選用）

如果您計劃將 API 部署到 Azure App Service，可以同時啟用：

在「防火牆與虛擬網路」設定中：
- ✅ 勾選「允許 Azure 服務和資源存取此伺服器」

> [!WARNING]
> 這會允許所有 Azure 服務連線，建議只在測試環境或搭配其他安全措施使用。

---

## 驗證設定

設定完成後，請執行以下命令測試連線：

```powershell
cd c:\Users\VivoBook\Desktop\betterthanvieshow\betterthanvieshow
dotnet ef database update
```

**預期結果**:
```
Build started...
Build succeeded.
Applying migration 'InitialCreate'.
Done.
```

---

## 其他注意事項

### 動態 IP 問題

如果您的 ISP 提供的是動態 IP（會隨時間改變），您可能需要：

1. **定期更新防火牆規則**
   - 每次 IP 變更時更新

2. **使用 IP 範圍**
   - 設定較寬的 IP 範圍（例如 36.238.11.0 - 36.238.11.255）
   - ⚠️ 注意：這會降低安全性

3. **使用 VPN 或固定 IP**
   - 考慮使用企業 VPN 或申請固定 IP

### 安全性建議

- 🔒 只開放必要的 IP 地址
- 🔐 定期檢查並移除不再使用的防火牆規則
- 📊 啟用 Azure SQL Database 的審核功能
- 🔑 使用強密碼並定期更換

---

## 常見問題

### Q: 設定後仍然無法連線？

1. 確認防火牆規則已儲存
2. 等待 30 秒讓設定生效
3. 檢查 IP 地址是否正確
4. 確認連線字串正確

### Q: 如何查看目前的防火牆規則？

Azure Portal → SQL Server → 網路 → 防火牆規則列表

### Q: 可以同時允許多個 IP 嗎？

可以！為每個 IP 建立獨立的防火牆規則，或使用 IP 範圍。

---

## 下一步

設定完防火牆規則後：

1. ✅ 執行資料庫遷移
2. ✅ 測試會員註冊 API
3. ✅ 開始開發其他功能
