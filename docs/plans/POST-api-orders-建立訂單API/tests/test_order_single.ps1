$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjM1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoidGVzdDEyMzRAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImV4cCI6MTc2NzU1NDIyNiwiaXNzIjoiQmV0dGVyVGhhblZpZVNob3dBUEkiLCJhdWQiOiJCZXR0ZXJUaGFuVmllU2hvd0NsaWVudCJ9.PinwmQqrf9XYEJmhe7n7GXunc6oOkBRelmzZhV2tmBU"
$url = "http://localhost:5041/api/orders"
$body = @{
    showTimeId = 7
    seatIds    = @(3, 4)
} | ConvertTo-Json

Write-Host "Testing Create Order API..."
try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }
    Write-Host "Success!"
    $response | ConvertTo-Json -Depth 5
}
catch {
    Write-Host "Error: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader $_.Exception.Response.GetResponseStream()
        $msg = $reader.ReadToEnd()
        Write-Host "Details written to error.log"
        $msg | Out-File -FilePath "error.log" -Encoding utf8
    }
}
