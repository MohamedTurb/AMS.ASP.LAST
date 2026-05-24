Write-Host "Stopping any running AMS instance..."
Stop-Process -Name "AssistanceManagementSystem" -Force -ErrorAction SilentlyContinue
Get-NetTCPConnection -LocalPort 5002 -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
Start-Sleep -Seconds 1

Write-Host "Building and starting on http://localhost:5002 ..."
Set-Location $PSScriptRoot
dotnet run --project AssistanceManagementSystem.csproj
