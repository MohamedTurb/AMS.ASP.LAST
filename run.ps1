$httpPort = 5002
$httpsPort = 5003
Write-Host "Stopping any running AssistanceManagementSystem processes..."
Get-Process AssistanceManagementSystem -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }

Write-Host "Building and starting on http://localhost:$httpPort and https://localhost:$httpsPort ..."
Set-Location $PSScriptRoot
dotnet build AssistanceManagementSystem.csproj
if ($LASTEXITCODE -eq 0) {
    $env:ASPNETCORE_URLS = "http://localhost:$httpPort;https://localhost:$httpsPort"
    $env:ASPNETCORE_HTTPS_PORT = "$httpsPort"
    dotnet run --project AssistanceManagementSystem.csproj
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE"
}
