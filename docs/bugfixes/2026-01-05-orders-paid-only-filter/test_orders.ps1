# 簡單測試 - 檢查訂單狀態
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjM1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoidGVzdDEyMzRAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImV4cCI6MTc2ODIxNjExOCwiaXNzIjoiQmV0dGVyVGhhblZpZVNob3dBUEkiLCJhdWQiOiJCZXR0ZXJUaGFuVmllU2hvd0NsaWVudCJ9.A8KBjGsBiimuFHkp4YUcSu-mGD_uoLPVgN-pO-NZfHw"

$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "Testing GET /api/orders..." -ForegroundColor Cyan
$response = Invoke-RestMethod -Uri "http://localhost:5041/api/orders" -Method Get -Headers $headers

Write-Host "`nTotal Orders: $($response.data.Count)" -ForegroundColor Yellow

Write-Host "`nOrder Status Summary:" -ForegroundColor Cyan
foreach ($order in $response.data) {
    Write-Host "  Order $($order.orderId): Status = $($order.status)" -ForegroundColor White
}

# 驗證所有訂單都是 Paid
$nonPaidOrders = $response.data | Where-Object { $_.status -ne "Paid" }

Write-Host "`nValidation Result:" -ForegroundColor Cyan
if ($nonPaidOrders.Count -eq 0) {
    Write-Host "  PASS - All $($response.data.Count) orders have status 'Paid'" -ForegroundColor Green
}
else {
    Write-Host "  FAIL - Found $($nonPaidOrders.Count) non-Paid orders!" -ForegroundColor Red
    foreach ($order in $nonPaidOrders) {
        Write-Host "    Order $($order.orderId): $($order.status)" -ForegroundColor Red
    }
}
