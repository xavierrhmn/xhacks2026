# Tempo Hackathon Demo - Interactive Version
param([switch]$SkipBuild = $false)
$ErrorActionPreference = "Stop"

function Write-Banner { 
    param([string]$Text)
    Write-Host ""
    Write-Host "======================================================================" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "======================================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Wait-ForEnter { 
    Write-Host ""
    Write-Host "----------------------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  Press ENTER to continue..." -ForegroundColor Yellow
    $null = Read-Host 
}

function Prompt-Command {
    param(
        [string]$ExpectedCommand,
        [string]$Hint = "",
        [switch]$Execute
    )
    
    Write-Host ""
    Write-Host "  Type this command:" -ForegroundColor Yellow
    Write-Host "  > " -NoNewline -ForegroundColor Green
    Write-Host $ExpectedCommand -ForegroundColor White
    if ($Hint) {
        Write-Host "  ($Hint)" -ForegroundColor DarkGray
    }
    Write-Host ""
    
    while ($true) {
        Write-Host "  $ " -NoNewline -ForegroundColor Cyan
        $userInput = Read-Host
        
        # Normalize: trim whitespace
        $userInput = $userInput.Trim()
        $expected = $ExpectedCommand.Trim()
        
        if ($userInput -eq $expected) {
            Write-Host "  Correct!" -ForegroundColor Green
            Write-Host ""
            if ($Execute) {
                Invoke-Expression $ExpectedCommand
            }
            return
        } else {
            Write-Host "  Try again. Expected: $ExpectedCommand" -ForegroundColor Red
        }
    }
}

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
Write-Host "  This is an INTERACTIVE demo" -ForegroundColor Magenta
Write-Host ""
Wait-ForEnter

# Build if needed
if (-not $SkipBuild) { 
    Write-Banner "Building Demo Application"
    Write-Host "Building DemoApp..." -ForegroundColor Gray
    Push-Location DemoApp
    dotnet build -c Release --verbosity quiet
    Pop-Location
    Write-Host "Build complete!" -ForegroundColor Green
    Wait-ForEnter 
}

# Demo 1: Clear history
Write-Banner "Fresh Start"
Write-Host "  First, clear any existing benchmark history." -ForegroundColor White
Prompt-Command -ExpectedCommand "tempo clear" -Execute
Write-Host "History cleared!" -ForegroundColor Green
Wait-ForEnter

# Demo 1.5: Warmup Demonstration
Write-Banner "Demo 1: Why Warmups Matter"
Write-Host "  First run without warmup - notice JIT compilation overhead:" -ForegroundColor White
Prompt-Command -ExpectedCommand "tempo run DemoApp/bin/Release/net8.0/DemoApp.exe fast --runs 3" -Hint "No warmup - first run will be slower" -Execute
Write-Host ""
Write-Host "  Now with warmup runs - JIT is already compiled:" -ForegroundColor White
Prompt-Command -ExpectedCommand "tempo run DemoApp/bin/Release/net8.0/DemoApp.exe fast --runs 3 --warmup 2" -Hint "2 warmups before measuring" -Execute
Write-Host ""
Write-Host "  Warmups eliminate cold-start variance for accurate measurements!" -ForegroundColor Yellow
Wait-ForEnter

# Demo 2: Statistical Benchmarking
Write-Banner "Demo 2: Statistical Benchmarking"
Write-Host "  Run a full benchmark with runs and warmups for statistical accuracy." -ForegroundColor White
Prompt-Command -ExpectedCommand "tempo run DemoApp/bin/Release/net8.0/DemoApp.exe fast --runs 5 --warmup 2" -Hint "5 runs + 2 warmups = reliable data" -Execute
Write-Host ""
Write-Host "  Notice: Mean, StdDev, P50/P95/P99 percentiles!" -ForegroundColor Yellow
Wait-ForEnter

# Demo 3: Building History (automated - too many commands)
Write-Banner "Demo 3: Building History"
Write-Host "  Simulating 7 commits with varying performance..." -ForegroundColor Gray
Write-Host "  (This part is automated to save time)" -ForegroundColor DarkGray
Write-Host ""
$commits = @(
    @{m="fast";d="Commit 1 - Fast";c="Green"},
    @{m="fast";d="Commit 2 - Fast";c="Green"},
    @{m="medium";d="Commit 3 - Medium";c="Yellow"},
    @{m="medium";d="Commit 4 - Medium";c="Yellow"},
    @{m="slow";d="Commit 5 - REGRESSION!";c="Red"},
    @{m="slow";d="Commit 6 - Still slow";c="Red"},
    @{m="fast";d="Commit 7 - Fixed!";c="Green"}
)
$i = 1
foreach ($c in $commits) {
    Write-Host "  [$i/7] $($c.d)" -ForegroundColor $c.c
    tempo run DemoApp/bin/Release/net8.0/DemoApp.exe $c.m --runs 3 2>&1 | Out-Null
    $i++
}
Write-Host ""
Write-Host "  History built!" -ForegroundColor Green
Wait-ForEnter

# Demo 4: Trend Analysis
Write-Banner "Demo 4: KILLER FEATURE - Trend Analysis"
Write-Host "  Now visualize the performance trend with ASCII charts!" -ForegroundColor White
Prompt-Command -ExpectedCommand "tempo trend DemoApp" -Execute
Write-Host ""
Write-Host "  Features: Linear regression, Sparklines, Histograms - all in terminal!" -ForegroundColor Cyan
Wait-ForEnter

# Demo 5: Baselines
Write-Banner "Demo 5: CI/CD Baselines"
Write-Host "  Set a baseline after a known-good build..." -ForegroundColor White
Prompt-Command -ExpectedCommand "tempo baseline set DemoApp" -Execute
Write-Host ""
Write-Host "  Now simulate a regression and compare against baseline..." -ForegroundColor Gray
tempo run DemoApp/bin/Release/net8.0/DemoApp.exe variable --runs 3 2>&1 | Out-Null
Write-Host ""
Prompt-Command -ExpectedCommand "tempo baseline compare DemoApp" -Execute
Write-Host ""
Write-Host "  Use --fail-on-regression in CI to fail builds automatically!" -ForegroundColor Yellow
Wait-ForEnter

# Demo 6: JSON Export
Write-Banner "Demo 6: JSON Export"
Write-Host "  Export data for dashboards, Grafana, or custom tooling:" -ForegroundColor White
Write-Host ""
Write-Host "  Type this command:" -ForegroundColor Yellow
Write-Host "  > " -NoNewline -ForegroundColor Green
Write-Host "tempo export DemoApp" -ForegroundColor White
Write-Host ""
Write-Host "  $ " -NoNewline -ForegroundColor Cyan
$userInput = Read-Host
if ($userInput.Trim() -eq "tempo export DemoApp") {
    Write-Host "  Correct!" -ForegroundColor Green
    tempo export DemoApp | Select-Object -First 15
    Write-Host "  ..." -ForegroundColor Gray
} else {
    Write-Host "  (Running command anyway)" -ForegroundColor DarkGray
    tempo export DemoApp | Select-Object -First 15
    Write-Host "  ..." -ForegroundColor Gray
}
Wait-ForEnter

# Demo 7: Web Dashboard
Write-Banner "Demo 7: Web Dashboard"
Write-Host "  We also built a beautiful web dashboard!" -ForegroundColor White
Write-Host "    - Chart.js visualizations" -ForegroundColor Cyan
Write-Host "    - Glassmorphism theme" -ForegroundColor Cyan
Write-Host "    - Real-time data from JSON exports" -ForegroundColor Cyan
Write-Host ""
Write-Host "  To start: " -NoNewline -ForegroundColor Gray
Write-Host "python -m http.server 8080" -ForegroundColor White
Write-Host "  Then open: " -NoNewline -ForegroundColor Gray
Write-Host "http://localhost:8080/Dashboard/" -ForegroundColor Green
Wait-ForEnter

# Closing
Write-Banner "Demo Complete!"
Write-Host "  Features:" -ForegroundColor White
Write-Host "  [x] Statistical benchmarking (mean, stddev, percentiles)" -ForegroundColor Green
Write-Host "  [x] Trend charts (ASCII line charts, sparklines, histograms)" -ForegroundColor Green
Write-Host "  [x] CI/CD baselines (--fail-on-regression)" -ForegroundColor Green
Write-Host "  [x] JSON export (for Grafana, DataDog, etc.)" -ForegroundColor Green
Write-Host "  [x] Web dashboard (Chart.js + glassmorphism)" -ForegroundColor Green
Write-Host "  [x] Configurable threshold (--threshold N)" -ForegroundColor Green
Write-Host ""
Write-Host "  Install with one command:" -ForegroundColor Yellow
Write-Host ""
Write-Host "    dotnet tool install --global Tempo" -ForegroundColor White
Write-Host ""
Write-Host "======================================================================" -ForegroundColor Magenta
Write-Host "      Thank you! Questions?" -ForegroundColor Magenta
Write-Host "======================================================================" -ForegroundColor Magenta
Write-Host ""
