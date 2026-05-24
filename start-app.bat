@echo off
echo Starting Assistance Management System on localhost:5006...
echo Current directory: %CD%

echo Building project...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Build failed with error code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo Build successful. Starting application...
echo Application will be available at: http://localhost:5006
echo Press Ctrl+C to stop the application
dotnet run --urls "http://localhost:5006"
