# PerfReg Dashboard

A browser-based dashboard for visualizing PerfReg benchmark results.

## Quick Start

1. **Start a local server** (from the `Perf_Regression_Detector` directory):
   ```bash
   # Python 3
   python -m http.server 8080
   
   # Or with Node.js
   npx serve .
   ```

2. **Open in browser**:
   ```
   http://localhost:8080/Dashboard/
   ```

## Features

- 📊 **Runtime & Memory Charts** - Interactive trend visualization
- 📈 **Trend Detection** - Automatic degradation/improvement detection  
- 📉 **Distribution Histogram** - See runtime distribution
- 🎯 **Percentile Display** - P50, P95, P99 values
- 📋 **Recent Runs Table** - Quick overview of history
- 🌙 **Dark Theme** - Beautiful glassmorphism design

## How It Works

The dashboard reads `.benchmark.json` files from the parent directory and displays the data using Chart.js. If no benchmark files are found, it shows sample data for demonstration.

## Requirements

- Modern web browser (Chrome, Firefox, Edge)
- Local HTTP server (file:// protocol has CORS limitations)
