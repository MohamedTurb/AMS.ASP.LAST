Write-Host "Starting Assistance Management System..."
try {
    dotnet run --urls "https://localhost:5001;http://localhost:5000"
} catch {
    Write-Host "Error running the application: $($_.Exception.Message)"
    Write-Host "Trying alternative approach..."
    dotnet build
    if ($LASTEXITCODE -eq 0) {
        dotnet run --no-build --urls "https://localhost:5001;http://localhost:5000"
    }
}
