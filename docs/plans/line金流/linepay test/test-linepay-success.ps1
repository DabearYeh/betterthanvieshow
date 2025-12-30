# LINE Pay Integration - Smart Test with Theater 14
# Using showtime from 12/28 and seats 37-56

$baseUrl = "https://better-than-vieshow-api.rocket-coding.com"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "LINE Pay Integration - Smart Test" -ForegroundColor Cyan
Write-Host "Theater: 大熊廳 (ID 14)" -ForegroundColor Cyan
Write-Host "Date: 12/28" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Login
Write-Host "[1] Login..." -ForegroundColor Yellow
try {
    $loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post `
        -Body '{"email":"test.customer@example.com","password":"Test123456!"}' `
        -ContentType "application/json"
    $token = $loginResp.data.token
    Write-Host "    OK`n" -ForegroundColor Green
}
catch {
    Write-Host "    FAILED`n" -ForegroundColor Red
    exit 1
}

# Showtime IDs from 12/28 (Theater 14)
$showtimeIds = @(10, 11, 12, 13, 14)

# Seat combinations to try (from seats 37-56)
$seatCombinations = @(
    @(37, 38),
    @(39, 40),
    @(41, 42),
    @(43, 44),
    @(45, 46),
    @(47, 48),
    @(49, 50),
    @(51, 52),
    @(53, 54),
    @(55, 56)
)

$orderCreated = $false
$orderId = 0
$orderNumber = ""

# Step 2: Try to create order
Write-Host "[2] Creating order..." -ForegroundColor Yellow

foreach ($showtimeId in $showtimeIds) {
    if ($orderCreated) { break }
    
    foreach ($seats in $seatCombinations) {
        try {
            $orderBody = @{
                showTimeId = $showtimeId
                seatIds    = $seats
            } | ConvertTo-Json
            
            $orderResp = Invoke-RestMethod -Uri "$baseUrl/api/orders" `
                -Method Post `
                -Headers @{Authorization = "Bearer $token" } `
                -Body $orderBody `
                -ContentType "application/json"
            
            $orderId = $orderResp.data.orderId
            $orderNumber = $orderResp.data.orderNumber
            $price = $orderResp.data.totalPrice
            
            Write-Host "    OK - Order created!" -ForegroundColor Green
            Write-Host "    Order Number: $orderNumber" -ForegroundColor White
            Write-Host "    Order ID: $orderId" -ForegroundColor White  
            Write-Host "    Showtime ID: $showtimeId" -ForegroundColor White
            Write-Host "    Seats: $($seats -join ', ')" -ForegroundColor White
            Write-Host "    Total Price: `$$price`n" -ForegroundColor White
            
            $orderCreated = $true
            break
            
        }
        catch {
            # Try next combination
            continue
        }
    }
}

if (-not $orderCreated) {
    Write-Host "    FAILED - All seat combinations occupied`n" -ForegroundColor Red
    exit 1
}

# Step 3: Request LINE Pay
Write-Host "[3] Requesting LINE Pay payment..." -ForegroundColor Yellow
try {
    $payBody = @{orderId = $orderId } | ConvertTo-Json
    
    $payResp = Invoke-RestMethod -Uri "$baseUrl/api/payments/line-pay/request" `
        -Method Post `
        -Headers @{Authorization = "Bearer $token" } `
        -Body $payBody `
        -ContentType "application/json"
    
    $transactionId = $payResp.data.transactionId
    $paymentUrl = $payResp.data.paymentUrl
    
    Write-Host "    OK - Payment request successful!`n" -ForegroundColor Green
    
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host " SUCCESS! LINE Pay Integration is WORKING!" -ForegroundColor Green
    Write-Host "============================================================`n" -ForegroundColor Green
    
    Write-Host "Transaction Details:" -ForegroundColor Cyan
    Write-Host "  Transaction ID: $transactionId" -ForegroundColor White
    Write-Host "  Order Number: $orderNumber" -ForegroundColor White
    Write-Host "`nPayment URL:" -ForegroundColor Cyan
    Write-Host "  $paymentUrl`n" -ForegroundColor Yellow
    
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Open the payment URL in your browser" -ForegroundColor White
    Write-Host "  2. Log in with LINE Pay Sandbox account" -ForegroundColor White
    Write-Host "  3. Complete the payment process" -ForegroundColor White
    Write-Host "  4. You will be redirected to:" -ForegroundColor White
    Write-Host "     https://better-than-vieshow-user.vercel.app/checkout/confirm" -ForegroundColor Gray
    
    # Try to copy URL to clipboard
    try {
        Set-Clipboard -Value $paymentUrl
        Write-Host "`n  Payment URL copied to clipboard!" -ForegroundColor Green
    }
    catch {
        # Clipboard not available
    }
    
}
catch {
    Write-Host "    FAILED`n" -ForegroundColor Red
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errBody = $reader.ReadToEnd()
    Write-Host "Error Details:" -ForegroundColor Yellow
    Write-Host $errBody -ForegroundColor White
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
