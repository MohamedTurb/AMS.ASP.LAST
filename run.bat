@echo off
echo Stopping any running AssistanceManagementSystem processes...
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq AssistanceManagementSystem.exe" /NH') do (
	taskkill /PID %%a /F >nul 2>&1
)

echo Building AssistanceManagementSystem...
dotnet build AssistanceManagementSystem.csproj
if %ERRORLEVEL% EQU 0 (
	echo Starting on http://localhost:5002 and https://localhost:5003...
	set ASPNETCORE_URLS=http://localhost:5002;https://localhost:5003
	set ASPNETCORE_HTTPS_PORT=5003
	dotnet run --project AssistanceManagementSystem.csproj
) else (
	echo Build failed with exit code %ERRORLEVEL%
)
pause
