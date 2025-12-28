$baseUrl = "http://localhost:5041"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjM1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoidGVzdDEyMzRAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsImV4cCI6MTc2NzU1NDIyNiwiaXNzIjoiQmV0dGVyVGhhblZpZVNob3dBUEkiLCJhdWQiOiJCZXR0ZXJUaGFuVmllU2hvd0NsaWVudCJ9.PinwmQqrf9XYEJmhe7n7GXunc6oOkBRelmzZhV2tmBU"

function Log-Msg($msg) {
    Write-Host $msg
    $msg | Out-File -FilePath "test_full_results.log" -Append -Encoding utf8
}

function Test-Api($name, $body, $expectedStatus) {
    Log-Msg "`n[$name] Testing..." 
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Body ($body | ConvertTo-Json) -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }
        
        if ($expectedStatus -eq 201) {
            Log-Msg " [PASS] Success (201)"
            # Write-Host ($response | ConvertTo-Json -Depth 2)
        }
        else {
            Log-Msg " [FAIL] Expected $expectedStatus but got 201 Created"
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq $expectedStatus) {
            Log-Msg " [PASS] Got Expected Error ($statusCode)"
            $reader = New-Object System.IO.StreamReader $_.Exception.Response.GetResponseStream()
            Log-Msg "      Message: $($reader.ReadToEnd())"
        }
        else {
            Log-Msg " [FAIL] Expected $expectedStatus but got $statusCode"
            Log-Msg "      Error: $_"
        }
    }
}

# Clear log file
"" | Out-File -FilePath "test_full_results.log" -Encoding utf8

# 1. 測試重複訂位 (Conflict 409)
# 前提：座位 3 剛剛已經被訂走了
Test-Api "Scenario: Seat Occupied" @{ showTimeId = 7; seatIds = @(3) } 409

# 2. 測試場次不存在 (Not Found 404)
Test-Api "Scenario: Showtime Not Found" @{ showTimeId = 99999; seatIds = @(5) } 404

# 3. 測試座位不存在 (Bad Request 400 - Validation or 404 based on implementation)
# 根據 Service 實作，如果座位 ID 沒在 DB 找到，會拋出 InvalidOperationException
Test-Api "Scenario: Seat Not Found" @{ showTimeId = 7; seatIds = @(99999) } 404

# 4. 測試訂太多票 (Bad Request 400)
Test-Api "Scenario: Too Many Tickets (>6)" @{ showTimeId = 7; seatIds = @(10, 11, 12, 13, 14, 15, 16) } 400

# 5. 測試訂 0 張票 (Bad Request 400)
Test-Api "Scenario: Zero Tickets" @{ showTimeId = 7; seatIds = @() } 400

# 6. 測試正常訂位 (Success 201)
# 找一個還沒訂的座位，例如 5, 6
Test-Api "Scenario: Valid Booking" @{ showTimeId = 7; seatIds = @(7, 8) } 201
