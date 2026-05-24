$httpPort = 5002
$httpsPort = 5003
Write-Host "Starting Assistance Management System on http://localhost:$httpPort and https://localhost:$httpsPort..."
Write-Host "Current directory: $(Get-Location)"
Write-Host "Checking .NET version..."
dotnet --version

Write-Host "Stopping any running AssistanceManagementSystem processes..."
Get-Process AssistanceManagementSystem -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }

Write-Host "Building project AssistanceManagementSystem.csproj..."
dotnet build AssistanceManagementSystem.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Starting application..."
    $env:ASPNETCORE_URLS = "http://localhost:$httpPort;https://localhost:$httpsPort"
    $env:ASPNETCORE_HTTPS_PORT = "$httpsPort"
    dotnet run --project AssistanceManagementSystem.csproj
} else {
    Write-Host "Build failed. Error code: $LASTEXITCODE"
}
