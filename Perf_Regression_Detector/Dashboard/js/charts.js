// PerfReg Dashboard - Chart Configuration Module

const Charts = {
    runtimeChart: null,
    memoryChart: null,
    distributionChart: null,

    // Chart.js default options for dark theme
    defaultOptions: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false
            },
            tooltip: {
                backgroundColor: 'rgba(20, 20, 40, 0.9)',
                titleColor: '#e8e8ff',
                bodyColor: '#a0a0c0',
                borderColor: 'rgba(100, 100, 200, 0.3)',
                borderWidth: 1,
                padding: 12,
                cornerRadius: 8
            }
        },
        scales: {
            x: {
                grid: {
                    color: 'rgba(100, 100, 200, 0.1)',
                    drawBorder: false
                },
                ticks: {
                    color: '#6060a0',
                    maxRotation: 45,
                    font: { size: 11 }
                }
            },
            y: {
                grid: {
                    color: 'rgba(100, 100, 200, 0.1)',
                    drawBorder: false
                },
                ticks: {
                    color: '#6060a0',
                    font: { size: 11 }
                }
            }
        }
    },

    // Initialize all charts
    init() {
        this.initRuntimeChart();
        this.initMemoryChart();
        this.initDistributionChart();
    },

    // Runtime trend line chart
    initRuntimeChart() {
        const ctx = document.getElementById('runtime-chart').getContext('2d');

        this.runtimeChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Runtime (ms)',
                    data: [],
                    borderColor: '#00d4ff',
                    backgroundColor: 'rgba(0, 212, 255, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#00d4ff',
                    pointBorderColor: '#0a0a1a',
                    pointBorderWidth: 2,
                    pointRadius: 5,
                    pointHoverRadius: 8
                }]
            },
            options: {
                ...this.defaultOptions,
                plugins: {
                    ...this.defaultOptions.plugins,
                    tooltip: {
                        ...this.defaultOptions.plugins.tooltip,
                        callbacks: {
                            label: (context) => `Runtime: ${context.raw.toFixed(2)} ms`
                        }
                    }
                },
                scales: {
                    ...this.defaultOptions.scales,
                    x: {
                        ...this.defaultOptions.scales.x,
                        title: {
                            display: true,
                            text: 'Time',
                            color: '#6060a0',
                            font: { size: 12, weight: 'bold' }
                        }
                    },
                    y: {
                        ...this.defaultOptions.scales.y,
                        title: {
                            display: true,
                            text: 'Runtime (ms)',
                            color: '#6060a0',
                            font: { size: 12, weight: 'bold' }
                        }
                    }
                }
            }
        });
    },

    // Memory usage line chart
    initMemoryChart() {
        const ctx = document.getElementById('memory-chart').getContext('2d');

        this.memoryChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Memory (MB)',
                    data: [],
                    borderColor: '#7c4dff',
                    backgroundColor: 'rgba(124, 77, 255, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#7c4dff',
                    pointBorderColor: '#0a0a1a',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 7
                }]
            },
            options: {
                ...this.defaultOptions,
                plugins: {
                    ...this.defaultOptions.plugins,
                    tooltip: {
                        ...this.defaultOptions.plugins.tooltip,
                        callbacks: {
                            label: (context) => `Memory: ${context.raw.toFixed(2)} MB`
                        }
                    }
                },
                scales: {
                    ...this.defaultOptions.scales,
                    x: {
                        ...this.defaultOptions.scales.x,
                        title: {
                            display: true,
                            text: 'Time',
                            color: '#6060a0',
                            font: { size: 12, weight: 'bold' }
                        }
                    },
                    y: {
                        ...this.defaultOptions.scales.y,
                        title: {
                            display: true,
                            text: 'Memory (MB)',
                            color: '#6060a0',
                            font: { size: 12, weight: 'bold' }
                        }
                    }
                }
            }
        });
    },

    // Runtime distribution histogram
    initDistributionChart() {
        const ctx = document.getElementById('distribution-chart').getContext('2d');

        this.distributionChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Runs',
                    data: [],
                    backgroundColor: 'rgba(0, 230, 118, 0.6)',
                    borderColor: '#00e676',
                    borderWidth: 1,
                    borderRadius: 4
                }]
            },
            options: {
                ...this.defaultOptions,
                plugins: {
                    ...this.defaultOptions.plugins,
                    tooltip: {
                        ...this.defaultOptions.plugins.tooltip,
                        callbacks: {
                            label: (context) => `${context.raw} runs`
                        }
                    }
                },
                scales: {
                    ...this.defaultOptions.scales,
                    x: {
                        ...this.defaultOptions.scales.x,
                        title: {
                            display: true,
                            text: 'Runtime Range (ms)',
                            color: '#6060a0',
                            font: { size: 12, weight: 'bold' }
                        }
                    },
                    y: {
                        ...this.defaultOptions.scales.y,
                        title: {
                            display: true,
                            text: 'Frequency',
                            color: '#6060a0',
                            font: { size: 12, weight: 'bold' }
                        }
                    }
                }
            }
        });
    },

    // Update runtime chart with new data
    updateRuntimeChart(results) {
        if (!results || results.length === 0) return;

        const labels = results.map(r => DataLoader.formatTimestamp(r.Timestamp));
        const data = results.map(r => r.RuntimeMs);

        this.runtimeChart.data.labels = labels;
        this.runtimeChart.data.datasets[0].data = data;
        this.runtimeChart.update('none');
    },

    // Update memory chart with new data
    updateMemoryChart(results) {
        if (!results || results.length === 0) return;

        const labels = results.map(r => DataLoader.formatTimestamp(r.Timestamp));
        const data = results.map(r => r.PeakMemoryBytes / (1024 * 1024));

        this.memoryChart.data.labels = labels;
        this.memoryChart.data.datasets[0].data = data;
        this.memoryChart.update('none');
    },

    // Update distribution histogram
    updateDistributionChart(results) {
        if (!results || results.length === 0) return;

        const runtimes = results.map(r => r.RuntimeMs);
        const min = Math.min(...runtimes);
        const max = Math.max(...runtimes);
        const bucketCount = 8;
        const bucketSize = (max - min) / bucketCount;

        // Create buckets
        const buckets = new Array(bucketCount).fill(0);
        const labels = [];

        for (let i = 0; i < bucketCount; i++) {
            const start = min + i * bucketSize;
            const end = start + bucketSize;
            labels.push(`${start.toFixed(0)}-${end.toFixed(0)}`);
        }

        // Fill buckets
        runtimes.forEach(runtime => {
            const bucketIndex = Math.min(
                Math.floor((runtime - min) / bucketSize),
                bucketCount - 1
            );
            buckets[bucketIndex]++;
        });

        this.distributionChart.data.labels = labels;
        this.distributionChart.data.datasets[0].data = buckets;
        this.distributionChart.update('none');
    },

    // Update all charts
    updateAll(results) {
        this.updateRuntimeChart(results);
        this.updateMemoryChart(results);
        this.updateDistributionChart(results);
    }
};

// Export for use in other modules
window.Charts = Charts;
