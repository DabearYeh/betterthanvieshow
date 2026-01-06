# Test Ticket Scan API - PosterUrl Field Validation
# Usage: .\test_scan_posterurl.ps1 [TicketNumber]
# Example: .\test_scan_posterurl.ps1 "49322368"

param(
    [Parameter(Mandatory = $false)]
    [string]$TicketNumber = ""
)

$baseUrl = "http://localhost:5041"
$token = "" # Please paste your admin token here

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Ticket Scan API - PosterUrl Field Test" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if token is provided
if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Host "[ERROR] Token is not configured!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please follow these steps:" -ForegroundColor Yellow
    Write-Host "1. Login to get admin token:" -ForegroundColor Yellow
    Write-Host "   POST http://localhost:5041/api/auth/login" -ForegroundColor Gray
    Write-Host "2. Copy the token from response" -ForegroundColor Yellow
    Write-Host "3. Paste it in this script (line 8)" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# If no ticket number provided, show usage
if ([string]::IsNullOrWhiteSpace($TicketNumber)) {
    Write-Host "Usage: .\test_scan_posterurl.ps1 [TicketNumber]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Example:" -ForegroundColor Yellow
    Write-Host "  .\test_scan_posterurl.ps1 `"49322368`"" -ForegroundColor Gray
    Write-Host ""
    Write-Host "To find ticket numbers, run this SQL query:" -ForegroundColor Yellow
    Write-Host "  SELECT TOP 5 TicketNumber, Status FROM Ticket;" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "Testing Ticket: $TicketNumber" -ForegroundColor Yellow
Write-Host ""

try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Accept"        = "application/json"
    }
    
    $url = "$baseUrl/api/admin/tickets/scan?qrCode=$TicketNumber"
    Write-Host "Request URL: $url" -ForegroundColor Gray
    Write-Host ""
    
    $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get -ErrorAction Stop
    
    if ($response.success) {
        Write-Host "[SUCCESS] Ticket information retrieved!" -ForegroundColor Green
        Write-Host ""
        Write-Host "--------------------------------------------" -ForegroundColor DarkCyan
        
        $data = $response.data
        
        Write-Host "Ticket ID         : " -NoNewline; Write-Host $data.ticketId -ForegroundColor White
        Write-Host "Ticket Number     : " -NoNewline; Write-Host $data.ticketNumber -ForegroundColor White
        Write-Host "Status            : " -NoNewline; Write-Host $data.status -ForegroundColor White
        Write-Host "Movie Title       : " -NoNewline; Write-Host $data.movieTitle -ForegroundColor White
        
        Write-Host ""
        Write-Host "===== POSTER URL CHECK =====" -ForegroundColor Magenta
        
        if ($data.PSObject.Properties.Name -contains "posterUrl") {
            if ([string]::IsNullOrWhiteSpace($data.posterUrl)) {
                Write-Host "Poster URL        : " -NoNewline
                Write-Host "(empty)" -ForegroundColor Yellow
                Write-Host "Result            : " -NoNewline
                Write-Host "[WARN] Field exists but no value" -ForegroundColor Yellow
            }
            else {
                Write-Host "Poster URL        : " -NoNewline
                Write-Host $data.posterUrl -ForegroundColor Cyan
                Write-Host "Result            : " -NoNewline
                Write-Host "[PASS] Field exists with value" -ForegroundColor Green
            }
        }
        else {
            Write-Host "Poster URL        : " -NoNewline
            Write-Host "[FIELD NOT FOUND]" -ForegroundColor Red
            Write-Host "Result            : " -NoNewline
            Write-Host "[FAIL] posterUrl field missing!" -ForegroundColor Red
        }
        
        Write-Host "============================" -ForegroundColor Magenta
        Write-Host ""
        
        Write-Host "Show Date         : " -NoNewline; Write-Host $data.showDate -ForegroundColor White
        Write-Host "Show Time         : " -NoNewline; Write-Host $data.showTime -ForegroundColor White
        Write-Host "Seat              : " -NoNewline; Write-Host $data.seatLabel -ForegroundColor White
        Write-Host "Theater           : " -NoNewline; Write-Host "$($data.theaterName) ($($data.theaterType))" -ForegroundColor White
        
        Write-Host ""
        Write-Host "--------------------------------------------" -ForegroundColor DarkCyan
        Write-Host ""
        Write-Host "Full JSON Response:" -ForegroundColor Gray
        Write-Host "--------------------------------------------" -ForegroundColor DarkGray
        $data | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor Gray
        Write-Host "--------------------------------------------" -ForegroundColor DarkGray
        
    }
    else {
        Write-Host "[FAILED] API Response Error" -ForegroundColor Red
        Write-Host "Message: $($response.message)" -ForegroundColor Red
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    
    if ($statusCode -eq 404) {
        Write-Host "[404 NOT FOUND] Ticket does not exist" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Suggestions:" -ForegroundColor Yellow
        Write-Host "1. Check if the ticket number is correct" -ForegroundColor Yellow
        Write-Host "2. Query database for valid tickets:" -ForegroundColor Yellow
        Write-Host "   SELECT TOP 5 TicketNumber, Status FROM Ticket;" -ForegroundColor Gray
    } 
    elseif ($statusCode -eq 401) {
        Write-Host "[401 UNAUTHORIZED] Token may be expired" -ForegroundColor Red
        Write-Host "Please login again and update the token in the script" -ForegroundColor Red
    }
    else {
        Write-Host "[ERROR] Request failed" -ForegroundColor Red
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
        Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
