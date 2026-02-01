// PerfReg Dashboard - Data Loading Module

const DataLoader = {
    // Known benchmark files - will be dynamically discovered
    benchmarkFiles: [],
    currentProgram: null,
    benchmarkData: {},

    // Initialize by finding benchmark files
    async init() {
        // Try to load benchmark files from the parent directory
        // In a real scenario, this would need a server endpoint
        // For demo, we'll use a predefined list or fetch from a manifest
        await this.discoverBenchmarkFiles();
        return this.benchmarkFiles;
    },

    // Discover available benchmark files
    async discoverBenchmarkFiles() {
        // Try common benchmark file names
        const potentialFiles = [
            'DemoApp.benchmark.json',
            'TestProgram.benchmark.json',
            'MyApp.benchmark.json'
        ];

        this.benchmarkFiles = [];

        for (const file of potentialFiles) {
            try {
                const response = await fetch(`../${file}`);
                if (response.ok) {
                    const data = await response.json();
                    this.benchmarkFiles.push(file.replace('.benchmark.json', ''));
                    this.benchmarkData[file.replace('.benchmark.json', '')] = data;
                }
            } catch (e) {
                // File doesn't exist, skip
            }
        }

        // If no files found, try to create sample data for demo
        if (this.benchmarkFiles.length === 0) {
            console.log('No benchmark files found. Using sample data for demo.');
            this.loadSampleData();
        }

        return this.benchmarkFiles;
    },

    // Load sample data for demonstration
    loadSampleData() {
        const sampleData = {
            ProgramName: "DemoApp",
            Results: this.generateSampleResults(15)
        };
        
        this.benchmarkFiles = ['DemoApp'];
        this.benchmarkData['DemoApp'] = sampleData;
    },

    // Generate sample benchmark results
    generateSampleResults(count) {
        const results = [];
        const baseTime = Date.now() - (count * 3600000); // Start from count hours ago
        
        for (let i = 0; i < count; i++) {
            const runtime = 150 + Math.random() * 100 + (i * 2); // Slight upward trend
            const memory = 14 + Math.random() * 3;
            
            results.push({
                CommitHash: this.randomHash(),
                Timestamp: new Date(baseTime + i * 3600000).toISOString(),
                RuntimeMs: runtime,
                PeakMemoryBytes: memory * 1024 * 1024,
                Gen0Collections: Math.floor(Math.random() * 3),
                Gen1Collections: Math.floor(Math.random() * 2),
                Gen2Collections: 0,
                Statistics: {
                    TotalRuns: 10,
                    WarmupRuns: 3,
                    Runtime: {
                        Mean: runtime,
                        Median: runtime - 5 + Math.random() * 10,
                        StdDev: 5 + Math.random() * 15,
                        Min: runtime - 20,
                        Max: runtime + 30,
                        P50: runtime - 2,
                        P95: runtime + 20,
                        P99: runtime + 28
                    },
                    Memory: {
                        Mean: memory * 1024 * 1024,
                        Median: memory * 1024 * 1024,
                        StdDev: 0.1 * 1024 * 1024,
                        Min: (memory - 0.2) * 1024 * 1024,
                        Max: (memory + 0.3) * 1024 * 1024,
                        P50: memory * 1024 * 1024,
                        P95: (memory + 0.2) * 1024 * 1024,
                        P99: (memory + 0.25) * 1024 * 1024
                    }
                }
            });
        }
        
        return results;
    },

    randomHash() {
        return Math.random().toString(16).substring(2, 9);
    },

    // Get data for a specific program
    getProgramData(programName) {
        return this.benchmarkData[programName] || null;
    },

    // Get list of programs
    getPrograms() {
        return this.benchmarkFiles;
    },

    // Calculate statistics from results
    calculateStats(results) {
        if (!results || results.length === 0) {
            return null;
        }

        const runtimes = results.map(r => r.RuntimeMs);
        const memories = results.map(r => r.PeakMemoryBytes / (1024 * 1024)); // Convert to MB

        const avgRuntime = runtimes.reduce((a, b) => a + b, 0) / runtimes.length;
        const avgMemory = memories.reduce((a, b) => a + b, 0) / memories.length;

        // Calculate trend (comparing first half to second half)
        const midpoint = Math.floor(runtimes.length / 2);
        const firstHalfAvg = runtimes.slice(0, midpoint).reduce((a, b) => a + b, 0) / midpoint;
        const secondHalfAvg = runtimes.slice(midpoint).reduce((a, b) => a + b, 0) / (runtimes.length - midpoint);
        const trendPercent = ((secondHalfAvg - firstHalfAvg) / firstHalfAvg) * 100;

        let trend = 'stable';
        if (trendPercent > 5) trend = 'degrading';
        else if (trendPercent < -5) trend = 'improving';

        return {
            totalRuns: results.length,
            avgRuntime: avgRuntime.toFixed(2),
            avgMemory: avgMemory.toFixed(2),
            trend,
            trendPercent: trendPercent.toFixed(1)
        };
    },

    // Get latest result
    getLatestResult(programName) {
        const data = this.getProgramData(programName);
        if (!data || !data.Results || data.Results.length === 0) {
            return null;
        }
        return data.Results[data.Results.length - 1];
    },

    // Format timestamp for display
    formatTimestamp(isoString) {
        const date = new Date(isoString);
        return date.toLocaleString('en-US', {
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Format bytes to MB
    formatMemory(bytes) {
        return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    },

    // Format milliseconds
    formatRuntime(ms) {
        return ms.toFixed(2) + ' ms';
    }
};

// Export for use in other modules
window.DataLoader = DataLoader;
