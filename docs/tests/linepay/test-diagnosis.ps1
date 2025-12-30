# Detailed LINE Pay Error Diagnosis
$ErrorActionPreference = "Stop"
$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

try {
    # Login
    $loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body '{"email":"test.customer@example.com","password":"Test123456!"}' -ContentType "application/json"
    $token = $loginResp.data.token
    
    # Create Order
    $orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Headers @{Authorization = "Bearer $token" } -Body '{"showTimeId":7,"seatIds":[5,6]}' -ContentType "application/json"
    $orderId = $orderResp.data.id
    
    Write-Host "Order created: #$($orderResp.data.orderNumber) (ID: $orderId)"
    
    # Try LINE Pay Request and capture full error
    try {
        $payResp = Invoke-WebRequest -Uri "$baseUrl/api/payments/line-pay/request" -Method Post -Headers @{Authorization = "Bearer $token" } -Body "{`"orderId`":$orderId}" -ContentType "application/json"
        Write-Host "SUCCESS! Payment URL: $($payResp.Content)"
    }
    catch {
        Write-Host "`n===== FULL ERROR DETAILS =====`n" -ForegroundColor Red
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Yellow
        Write-Host "Status Description: $($_.Exception.Response.StatusDescription)" -ForegroundColor Yellow
        
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $responseBody = $reader.ReadToEnd()
        
        Write-Host "`nResponse Body:" -ForegroundColor Yellow
        Write-Host $responseBody -ForegroundColor White
        
        # Try to parse as JSON for prettier output
        try {
            $jsonError = $responseBody | ConvertFrom-Json
            Write-Host "`nParsed Error:" -ForegroundColor Yellow
            Write-Host "Message: $($jsonError.message)" -ForegroundColor White
            if ($jsonError.detail) {
                Write-Host "Detail: $($jsonError.detail)" -ForegroundColor White
            }
            if ($jsonError.errors) {
                Write-Host "Errors: $($jsonError.errors | ConvertTo-Json)" -ForegroundColor White
            }
        }
        catch {
            Write-Host "Could not parse as JSON" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "Test failed before LINE Pay request: $($_.Exception.Message)" -ForegroundColor Red
}
