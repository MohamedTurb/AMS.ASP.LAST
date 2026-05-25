Param()

If (-Not (Test-Path -Path ".env")) {
    Write-Error ".env file not found. Copy .env.example to .env and fill values."
    exit 1
}

docker compose -f docker-compose.yml pull | Out-Null
docker compose -f docker-compose.yml up --build -d

Write-Host "Waiting for health endpoint..."
for ($i=0; $i -lt 20; $i++) {
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:5002/health" -UseBasicParsing -ErrorAction Stop
        if ($r.StatusCode -eq 200) {
            Write-Host "Service is healthy"
            exit 0
        }
    } catch {}
    Start-Sleep -Seconds 3
}

Write-Error "Service did not become healthy in time."
exit 2
