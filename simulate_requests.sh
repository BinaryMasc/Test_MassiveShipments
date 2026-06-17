#!/bin/bash

# Configuration
URL="http://localhost:5036/api/v1/webhooks/shipments"
TOKEN="static-demo-token-12345"

echo "========================================="
echo "Simulating Webhook Requests (Rate Limit: 10/min)"
echo "Target URL: $URL"
echo "========================================="

# Loop to send 12 requests (10 should pass, 2 should fail with 429)
for i in {1..12}
do
  echo "Sending Request $i..."
  
  PAYLOAD=$(cat <<EOF
{
  "eventId": "EVT-100$i",
  "trackingNumber": "TRK123456789",
  "status": "InTransit",
  "timestamp": "2023-10-27T10:00:00Z",
  "providerId": "PROV-01"
}
EOF
)

  # Execute curl and capture HTTP status code
  RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$URL" \
       -H "Content-Type: application/json" \
       -H "X-Webhook-Token: $TOKEN" \
       -d "$PAYLOAD")
       
  echo "Response Status Code: $RESPONSE"
  
  if [ "$RESPONSE" = "202" ]; then
    echo "✅ Accepted (202)"
  elif [ "$RESPONSE" = "429" ]; then
    echo "❌ Rate Limit Exceeded (429)"
  else
    echo "⚠️ Unexpected Status Code: $RESPONSE"
  fi
  
  echo "-----------------"
  # Small delay between requests to not overwhelm curl itself, but fast enough to hit rate limit
  sleep 0.2
done

echo ""
echo "Simulation Finished. Check the API console output to see the background worker processing the 10 accepted requests with a 2-second delay each."
