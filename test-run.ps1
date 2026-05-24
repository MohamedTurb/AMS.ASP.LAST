Write-Host "Testing application startup..."
Write-Host "Current directory: $(Get-Location)"

# Try to run the application
Write-Host "Starting application on localhost:5006..."
$process = Start-Process -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:5006" -PassThru -NoNewWindow

# Wait a moment for startup
Start-Sleep -Seconds 5

# Check if the process is running
if ($process -and !$process.HasExited) {
    Write-Host "Application started successfully!"
    Write-Host "Process ID: $($process.Id)"
    
    # Test the connection
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5006" -TimeoutSec 10
        Write-Host "Application is responding! Status: $($response.StatusCode)"
    } catch {
        Write-Host "Application is not responding: $($_.Exception.Message)"
    }
} else {
    Write-Host "Application failed to start"
}

# Keep the process running
Write-Host "Press any key to stop the application..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
$process.Kill()
