# Tempo - Hackathon Submission

## Inspiration

Look, here's the thing about performance regressions: they're invisible. You can write a thousand unit tests, get 100% code coverage, pass every linter known to humanity, and STILL ship code that makes your application 50% slower. Why? Because nobody's actually measuring it.

I've watched teams ship code for months, completely unaware that their "optimized" refactoring was actually a regression. By the time someone notices the app is slow, you're digging through six months of commits trying to find when things went wrong. That's insane. We automated everything else in the build pipeline - why not this?

## What it does

Tempo runs your program, measures its actual performance, and tells you when you've made it slower. Simple as that.

It runs multiple iterations with warmups (because your first run is always garbage due to cold caches), gives you real statistics - mean, standard deviation, P50, P95, P99 - and compares against your previous runs. If you regress by more than your threshold (default 5%), it yells at you.

The killer feature is the trend analysis: ASCII charts right in your terminal showing performance over time. Linear regression to detect gradual decay. Sparklines for a quick visual. No external dashboards, no cloud services, no 47 dependencies. Just run `tempo trend` and see exactly what your code is doing.

It works with any executable - not just .NET. If it runs and exits, Tempo can measure it.

## How we built it

.NET 8, no frameworks. The core is maybe 500 lines of actual logic. Process metrics come from the OS - we spawn your program, poll for peak memory usage, measure wall-clock time, grab GC data from the runtime. Store everything in JSON files (because why overcomplicate storage when flat files work fine).

The statistics code is straightforward: collect N samples, sort them, calculate percentiles. The trend analyzer does linear regression on your historical data to detect if you're getting faster or slower over time.

The ASCII charts are just character arrays. Map your values to a grid, draw lines between points, render to console. It's the kind of thing that sounds hard until you actually sit down and do it.

## Challenges we ran into

Windows console encoding was a pain. The sparkline characters (▁▂▃▄▅▆▇█) would show up as question marks until we explicitly set UTF-8 output encoding. Classic Windows.

Getting reliable measurements is harder than it sounds. First run is always slow (cold cache, JIT compilation). You need warmup runs. You need multiple samples to calculate meaningful statistics. One measurement is noise - ten measurements is data.

The biggest challenge was scope creep. We could have built a cloud dashboard, added machine learning for anomaly detection, integrated with seventeen CI systems. Instead we shipped something that actually works.

## Accomplishments that we're proud of

The ASCII trend charts. You run `tempo trend` and get a full line chart of your performance history, right in the terminal. No browser required. It looks good and it's actually useful - you can see exactly when a regression happened.

The whole thing is a single global tool install. `dotnet tool install --global Tempo`. Done. No configuration files required, no setup wizard, no account creation. Run a benchmark in 5 seconds.

Works with any executable. Python script? Node app? Rust binary? Don't care. If it runs, we measure it.

## What we learned

Most performance tools are overengineered. They try to do everything, require complex setup, and produce reports nobody reads. The tools people actually use are simple and fast.

Statistical sampling matters more than people think. One benchmark run tells you almost nothing - the variance is too high. Five runs with two warmups gives you actionable data.

ASCII graphics are underrated. There's something satisfying about seeing a chart rendered in your terminal without launching a browser or installing a GUI application.

## What's next for Tempo

- **More output formats** - GitHub Actions annotations would be slick
- **Benchmark suites** - Run multiple programs and track them together
- **Lightweight web dashboard** - For teams that want one. But optional. The CLI stays the priority

The core goal stays the same: catch performance regressions before your users do. Everything else is secondary.
