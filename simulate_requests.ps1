# Configuration

# If execution policy error: bypass it for that session using:
# Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass 
# or by running 
# powershell -ExecutionPolicy Bypass -File .\simulate_requests.ps1

$URL = "http://localhost:5036/api/v1/webhooks/shipments"
$TOKEN = "static-demo-token-12345"

Write-Output "========================================="
Write-Output "Simulating Webhook Requests (Rate Limit: 10/min)"
Write-Output "Target URL: $URL"
Write-Output "========================================="

# Loop to send 12 requests (10 should pass, 2 should fail with 429)
for ($i = 1; $i -le 12; $i++) {
    Write-Output "Sending Request $i..."

    $payload = @{
        eventId = "EVT-100$i"
        trackingNumber = "TRK123456789"
        status = "InTransit"
        timestamp = "2023-10-27T10:00:00Z"
        providerId = "PROV-01"
    } | ConvertTo-Json

    $headers = @{
        "X-Webhook-Token" = $TOKEN
    }

    $statusCode = $null
    try {
        $response = Invoke-WebRequest -Uri $URL -Method Post -Body $payload -ContentType "application/json" -Headers $headers -UseBasicParsing
        $statusCode = $response.StatusCode
    } catch {
        # Catch non-2xx HTTP responses (like 429 Rate Limit Exceeded)
        if ($_.Exception -and $_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        } else {
            $statusCode = "Error"
        }
    }

    Write-Output "Response Status Code: $statusCode"

    if ($statusCode -eq 202) {
        Write-Output "✅ Accepted (202)"
    } elseif ($statusCode -eq 429) {
        Write-Output "❌ Rate Limit Exceeded (429)"
    } else {
        Write-Output "⚠️ Unexpected Status Code: $statusCode"
    }

    Write-Output "-----------------"
    
    # Small delay between requests to not overwhelm, but fast enough to hit rate limit
    Start-Sleep -Milliseconds 200
}

Write-Output ""
Write-Output "Simulation Finished. Check the API console output to see the background worker processing the 10 accepted requests with a 2-second delay each."
