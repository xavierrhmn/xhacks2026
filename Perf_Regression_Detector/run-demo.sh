#!/bin/bash
# PerfReg Demo Script
# Quickly builds benchmark history and demonstrates features

echo "====================================="
echo "   PerfReg Demo Test Suite"
echo "====================================="
echo ""

# Clear previous demo data
echo "[1/7] Clearing previous demo data..."
dotnet run --project PerfReg clear
sleep 1

# Demo 1: Basic run with statistics
echo ""
echo "[2/7] Running basic benchmark with statistics..."
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
sleep 1

# Demo 2: Build history for trends
echo ""
echo "[3/7] Building benchmark history..."
for i in {1..5}; do
    echo "  Run $i/5..."
    dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 3 > /dev/null 2>&1
    sleep 0.5
done

# Add some variance
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 3 > /dev/null 2>&1
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 3 > /dev/null 2>&1
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 3 > /dev/null 2>&1

echo "  History built!"
sleep 1

# Demo 3: Show comparison
echo ""
echo "[4/7] Comparing last two runs..."
dotnet run --project PerfReg compare
sleep 2

# Demo 4: Set baseline
echo ""
echo "[5/7] Setting performance baseline..."
dotnet run --project PerfReg baseline set
sleep 1

# Demo 5: Show history
echo ""
echo "[6/7] Displaying benchmark history..."
dotnet run --project PerfReg history
sleep 2

# Demo 6: Show trends with charts (THE MONEY SHOT)
echo ""
echo "[7/7] Displaying trend analysis with charts..."
echo ""
echo "========================================"
echo "   TREND ANALYSIS (Main Feature!)"
echo "========================================"
echo ""
dotnet run --project PerfReg trend DemoApp

echo ""
echo "====================================="
echo "   Demo Complete!"
echo "====================================="
echo ""
echo "Try these commands manually:"
echo "  dotnet run --project PerfReg trend DemoApp"
echo "  dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe variable --runs 20"
echo "  dotnet run --project PerfReg baseline compare"
echo ""
