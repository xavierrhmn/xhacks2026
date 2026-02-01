# PerfReg Demo Script
# Quickly builds benchmark history and demonstrates features

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   PerfReg Demo Test Suite" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Clear previous demo data
Write-Host "[1/7] Clearing previous demo data..." -ForegroundColor Yellow
dotnet run --project PerfReg clear
Start-Sleep -Seconds 1

# Demo 1: Basic run with statistics
Write-Host ""
Write-Host "[2/7] Running basic benchmark with statistics..." -ForegroundColor Yellow
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
Start-Sleep -Seconds 1

# Demo 2: Build history for trends
Write-Host ""
Write-Host "[3/7] Building benchmark history..." -ForegroundColor Yellow
for ($i = 1; $i -le 5; $i++) {
    Write-Host "  Run $i/5..." -ForegroundColor Gray
    dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 3 | Out-Null
    Start-Sleep -Milliseconds 500
}

# Add some variance
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 3 | Out-Null
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 3 | Out-Null
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 3 | Out-Null

Write-Host "  History built!" -ForegroundColor Green
Start-Sleep -Seconds 1

# Demo 3: Show comparison
Write-Host ""
Write-Host "[4/7] Comparing last two runs..." -ForegroundColor Yellow
dotnet run --project PerfReg compare
Start-Sleep -Seconds 2

# Demo 4: Set baseline
Write-Host ""
Write-Host "[5/7] Setting performance baseline..." -ForegroundColor Yellow
dotnet run --project PerfReg baseline set
Start-Sleep -Seconds 1

# Demo 5: Show history
Write-Host ""
Write-Host "[6/7] Displaying benchmark history..." -ForegroundColor Yellow
dotnet run --project PerfReg history
Start-Sleep -Seconds 2

# Demo 6: Show trends with charts (THE MONEY SHOT)
Write-Host ""
Write-Host "[7/7] Displaying trend analysis with charts..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   TREND ANALYSIS (Main Feature!)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
dotnet run --project PerfReg trend DemoApp

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   Demo Complete!" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Try these commands manually:" -ForegroundColor Yellow
Write-Host "  dotnet run --project PerfReg trend DemoApp" -ForegroundColor White
Write-Host "  dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe variable --runs 20" -ForegroundColor White
Write-Host "  dotnet run --project PerfReg baseline compare" -ForegroundColor White
Write-Host ""
