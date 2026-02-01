# Tempo Hackathon Demo
param([switch]$SkipBuild = $false)
$ErrorActionPreference = "Stop"

function Write-Banner { param([string]$Text); Write-Host ""; Write-Host "======================================================================" -ForegroundColor Cyan; Write-Host "  $Text" -ForegroundColor Cyan; Write-Host "======================================================================" -ForegroundColor Cyan; Write-Host "" }
function Wait-ForEnter { Write-Host ""; Write-Host "----------------------------------------------------------------------" -ForegroundColor DarkGray; Write-Host "  Press ENTER to continue..." -ForegroundColor Yellow; $null = Read-Host }

Clear-Host
Write-Host ""
Write-Host "    ==============================" -ForegroundColor Magenta
Write-Host "         TEMPO" -ForegroundColor Cyan
Write-Host "         Performance Regression Detection" -ForegroundColor Cyan
Write-Host "         XHacks 2026" -ForegroundColor White
Write-Host "    ==============================" -ForegroundColor Magenta
Write-Host ""
Write-Host "  Catch performance regressions BEFORE they hit production" -ForegroundColor Yellow
Write-Host ""
Wait-ForEnter

if (-not $SkipBuild) { Write-Banner "Building Demo Application"; Write-Host "Building DemoApp..." -ForegroundColor Gray; Push-Location DemoApp; dotnet build -c Release --verbosity quiet; Pop-Location; Write-Host "Build complete!" -ForegroundColor Green; Wait-ForEnter }

Write-Banner "Fresh Start"; tempo clear; Write-Host "History cleared!" -ForegroundColor Green; Wait-ForEnter

Write-Banner "Demo 1: Statistical Benchmarking"; Write-Host "[1/6] Running benchmark..." -ForegroundColor Yellow; Write-Host ""; tempo run DemoApp/bin/Release/net8.0/DemoApp.exe fast --runs 5 --warmup 2; Write-Host ""; Write-Host "Notice: Mean, StdDev, P50/P95/P99 percentiles!" -ForegroundColor Yellow; Wait-ForEnter

Write-Banner "Demo 2: Building History"; Write-Host "Simulating commits..." -ForegroundColor Gray
$commits = @(@{m="fast";d="Commit 1";c="Green"},@{m="fast";d="Commit 2";c="Green"},@{m="medium";d="Commit 3";c="Yellow"},@{m="medium";d="Commit 4";c="Yellow"},@{m="slow";d="Commit 5 REGRESSION";c="Red"},@{m="slow";d="Commit 6";c="Red"},@{m="fast";d="Commit 7 Fixed";c="Green"})
$i=1; foreach($c in $commits){Write-Host "  [$i/7] $($c.d)" -ForegroundColor $c.c; tempo run DemoApp/bin/Release/net8.0/DemoApp.exe $c.m --runs 3 2>&1|Out-Null; $i++}
Write-Host ""; Write-Host "History built!" -ForegroundColor Green; Wait-ForEnter

Write-Banner "Demo 3: KILLER FEATURE - Trend Analysis"; Write-Host "ASCII charts in terminal!" -ForegroundColor White; Write-Host ""; tempo trend DemoApp; Write-Host ""; Write-Host "Features: Linear regression, Sparklines, Histograms" -ForegroundColor Cyan; Wait-ForEnter

Write-Banner "Demo 4: CI/CD Baselines"; Write-Host "[2/6] Setting baseline..." -ForegroundColor Yellow; tempo baseline set DemoApp; Write-Host "[3/6] New benchmark..." -ForegroundColor Yellow; tempo run DemoApp/bin/Release/net8.0/DemoApp.exe variable --runs 3 2>&1|Out-Null; Write-Host ""; Write-Host "[4/6] Comparing..." -ForegroundColor Yellow; Write-Host ""; tempo baseline compare DemoApp; Write-Host ""; Write-Host "Use --fail-on-regression in CI!" -ForegroundColor Yellow; Wait-ForEnter

Write-Banner "Demo 5: JSON Export"; Write-Host "Command: tempo export DemoApp" -ForegroundColor Cyan; Write-Host ""; tempo export DemoApp | Select-Object -First 10; Write-Host "..." -ForegroundColor Gray; Wait-ForEnter

Write-Banner "Demo 6: Web Dashboard"; Write-Host "Beautiful web dashboard!" -ForegroundColor White; Write-Host "  - Chart.js visualizations" -ForegroundColor Cyan; Write-Host "  - Glassmorphism theme" -ForegroundColor Cyan; Write-Host "  Start: python -m http.server 8080" -ForegroundColor Gray; Write-Host "  Open: http://localhost:8080/Dashboard/" -ForegroundColor Green; Wait-ForEnter

Write-Banner "Demo Complete!"; Write-Host "  Features:" -ForegroundColor White; Write-Host "  [x] Benchmarking" -ForegroundColor Green; Write-Host "  [x] Trend charts" -ForegroundColor Green; Write-Host "  [x] Baselines" -ForegroundColor Green; Write-Host "  [x] JSON export" -ForegroundColor Green; Write-Host "  [x] Dashboard" -ForegroundColor Green; Write-Host ""; Write-Host "  Install: dotnet tool install --global Tempo" -ForegroundColor Yellow; Write-Host ""; Write-Host "======================================================================" -ForegroundColor Magenta; Write-Host "      Thank you! Questions?" -ForegroundColor Magenta; Write-Host "======================================================================" -ForegroundColor Magenta
