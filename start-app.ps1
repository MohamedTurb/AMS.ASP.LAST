Write-Host "Starting Assistance Management System on localhost:5006..."
Write-Host "Current directory: $(Get-Location)"
Write-Host "Checking .NET version..."
dotnet --version

Write-Host "Building project..."
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Starting application..."
    dotnet run --urls "http://localhost:5006"
} else {
    Write-Host "Build failed. Error code: $LASTEXITCODE"
}
