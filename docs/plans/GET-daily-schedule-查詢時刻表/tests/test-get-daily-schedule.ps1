# 測試 GET Daily Schedule API

# 設定基礎 URL
$baseUrl = "https://localhost:7171"

# 忽略 SSL 憑證錯誤（僅用於開發環境）
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

Write-Host "=== 步驟 1：登入取得 Admin Token ===" -ForegroundColor Cyan

$loginBody = @{
    email = "admin@example.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ 登入成功，Token: $($token.Substring(0, 20))..." -ForegroundColor Green
} catch {
    Write-Host "✗ 登入失敗: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "`n=== 步驟 2：測試查詢不存在的時刻表 (應返回 404) ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2099-12-31" -Method GET -Headers $headers
    Write-Host "✗ 應該返回 404，但回傳了資料" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "✓ 正確返回 404 Not Found" -ForegroundColor Green
    } else {
        Write-Host "✗ 錯誤的狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n=== 步驟 3：測試日期格式錯誤 (應返回 400) ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/invalid-date" -Method GET -Headers $headers
    Write-Host "✗ 應該返回 400，但回傳了資料" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ 正確返回 400 Bad Request" -ForegroundColor Green
    } else {
        Write-Host "✗ 錯誤的狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n=== 步驟 4：測試未授權訪問 (應返回 401) ===" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/daily-schedules/2025-12-24" -Method GET
    Write-Host "✗ 應該返回 401，但回傳了資料" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ 正確返回 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ 錯誤的狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n=== 步驟 5：查詢存在的時刻表 (需要先建立資料) ===" -ForegroundColor Cyan
Write-Host "提示：如果資料庫中有時刻表資料，請手動測試以下日期：" -ForegroundColor Yellow
Write-Host "  GET $baseUrl/api/admin/daily-schedules/2025-12-24" -ForegroundColor Yellow

Write-Host "`n=== 測試完成 ===" -ForegroundColor Cyan
