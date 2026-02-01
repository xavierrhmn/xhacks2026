// PerfReg Dashboard - Main Application

const App = {
    currentProgram: null,

    // Initialize the application
    async init() {
        console.log('PerfReg Dashboard initializing...');

        // Initialize data loader
        await DataLoader.init();

        // Initialize charts
        Charts.init();

        // Populate program selector
        this.populateProgramSelector();

        // Set up event listeners
        this.setupEventListeners();

        // Load initial data
        const programs = DataLoader.getPrograms();
        if (programs.length > 0) {
            this.currentProgram = programs[0];
            this.loadProgramData(this.currentProgram);
        }

        console.log('Dashboard initialized successfully!');
    },

    // Populate the program dropdown
    populateProgramSelector() {
        const select = document.getElementById('program-select');
        const programs = DataLoader.getPrograms();

        select.innerHTML = '';

        if (programs.length === 0) {
            select.innerHTML = '<option value="">No programs found</option>';
            return;
        }

        programs.forEach(program => {
            const option = document.createElement('option');
            option.value = program;
            option.textContent = program;
            select.appendChild(option);
        });
    },

    // Set up event listeners
    setupEventListeners() {
        // Program selector change
        document.getElementById('program-select').addEventListener('change', (e) => {
            this.currentProgram = e.target.value;
            this.loadProgramData(this.currentProgram);
        });

        // Refresh button
        document.getElementById('refresh-btn').addEventListener('click', async () => {
            await DataLoader.init();
            this.populateProgramSelector();
            if (this.currentProgram) {
                this.loadProgramData(this.currentProgram);
            }
        });
    },

    // Load and display data for a program
    loadProgramData(programName) {
        const data = DataLoader.getProgramData(programName);

        if (!data || !data.Results) {
            console.error('No data found for program:', programName);
            return;
        }

        const results = data.Results;

        // Update stats cards
        this.updateStatsCards(results);

        // Update charts
        Charts.updateAll(results);

        // Update percentiles
        this.updatePercentiles(results);

        // Update summary panel
        this.updateSummaryPanel(programName, results);

        // Update runs table
        this.updateRunsTable(results);
    },

    // Update the summary panel with latest run info
    updateSummaryPanel(programName, results) {
        const latest = results[results.length - 1];

        if (!latest) return;

        document.getElementById('summary-program').textContent = programName;
        document.getElementById('summary-timestamp').textContent =
            DataLoader.formatTimestamp(latest.Timestamp);
        document.getElementById('summary-runtime').textContent =
            latest.RuntimeMs.toFixed(2) + ' ms';
        document.getElementById('summary-memory').textContent =
            (latest.PeakMemoryBytes / (1024 * 1024)).toFixed(2) + ' MB';

        // Std Dev
        if (latest.Statistics && latest.Statistics.Runtime && latest.Statistics.Runtime.StdDev) {
            document.getElementById('summary-stddev').textContent =
                '±' + latest.Statistics.Runtime.StdDev.toFixed(2) + ' ms';
        } else {
            document.getElementById('summary-stddev').textContent = '--';
        }

        // Commit
        document.getElementById('summary-commit').textContent =
            latest.CommitHash ? latest.CommitHash.substring(0, 7) : '--';

        // GC Collections
        const gc0 = latest.Gen0Collections || 0;
        const gc1 = latest.Gen1Collections || 0;
        const gc2 = latest.Gen2Collections || 0;
        document.getElementById('summary-gc').textContent = `${gc0} / ${gc1} / ${gc2}`;
    },

    // Update the stats cards
    updateStatsCards(results) {
        const stats = DataLoader.calculateStats(results);

        if (!stats) return;

        document.getElementById('total-runs').textContent = stats.totalRuns;
        document.getElementById('avg-runtime').textContent = stats.avgRuntime + ' ms';
        document.getElementById('avg-memory').textContent = stats.avgMemory + ' MB';

        // Update trend
        const trendStatus = document.getElementById('trend-status');
        const trendIcon = document.getElementById('trend-icon');

        if (stats.trend === 'improving') {
            trendStatus.textContent = '↓ ' + Math.abs(stats.trendPercent) + '%';
            trendStatus.style.color = '#00e676';
            trendIcon.textContent = '📉';
        } else if (stats.trend === 'degrading') {
            trendStatus.textContent = '↑ ' + stats.trendPercent + '%';
            trendStatus.style.color = '#ff5252';
            trendIcon.textContent = '📈';
        } else {
            trendStatus.textContent = '→ Stable';
            trendStatus.style.color = '#a0a0c0';
            trendIcon.textContent = '📊';
        }
    },

    // Update percentiles display
    updatePercentiles(results) {
        const latest = results[results.length - 1];

        if (!latest || !latest.Statistics || !latest.Statistics.Runtime) {
            return;
        }

        const runtime = latest.Statistics.Runtime;

        document.getElementById('p50').textContent =
            runtime.P50 ? runtime.P50.toFixed(2) + ' ms' : '--';
        document.getElementById('p95').textContent =
            runtime.P95 ? runtime.P95.toFixed(2) + ' ms' : '--';
        document.getElementById('p99').textContent =
            runtime.P99 ? runtime.P99.toFixed(2) + ' ms' : '--';
        document.getElementById('min-val').textContent =
            runtime.Min ? runtime.Min.toFixed(2) + ' ms' : '--';
        document.getElementById('max-val').textContent =
            runtime.Max ? runtime.Max.toFixed(2) + ' ms' : '--';
    },

    // Update the runs table
    updateRunsTable(results) {
        const tbody = document.getElementById('runs-table-body');

        if (!results || results.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="loading">No runs found</td></tr>';
            return;
        }

        // Show last 10 runs in reverse order (newest first)
        const recentRuns = results.slice(-10).reverse();

        tbody.innerHTML = recentRuns.map((run, index) => {
            const runNumber = results.length - index;
            const runtime = run.RuntimeMs.toFixed(2);
            const memory = (run.PeakMemoryBytes / (1024 * 1024)).toFixed(2);
            const timestamp = DataLoader.formatTimestamp(run.Timestamp);
            const commit = run.CommitHash ? run.CommitHash.substring(0, 7) : '--';

            // Calculate change from previous run
            let changeHtml = '<span class="change-neutral">--</span>';
            if (index < recentRuns.length - 1) {
                const prevRun = recentRuns[index + 1];
                const change = ((run.RuntimeMs - prevRun.RuntimeMs) / prevRun.RuntimeMs) * 100;

                if (change > 5) {
                    changeHtml = `<span class="change-positive">↑ ${change.toFixed(1)}%</span>`;
                } else if (change < -5) {
                    changeHtml = `<span class="change-negative">↓ ${Math.abs(change).toFixed(1)}%</span>`;
                } else {
                    changeHtml = `<span class="change-neutral">→ ${change.toFixed(1)}%</span>`;
                }
            }

            return `
                <tr>
                    <td>${runNumber}</td>
                    <td>${timestamp}</td>
                    <td>${runtime} ms</td>
                    <td>${memory} MB</td>
                    <td><span class="commit-hash">${commit}</span></td>
                    <td>${changeHtml}</td>
                </tr>
            `;
        }).join('');
    }
};

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    App.init();
});

// Export for debugging
window.App = App;
