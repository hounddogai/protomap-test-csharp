namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkForecastReportChecks
{
    public static void BenchmarkForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707922665);
        var items = new List<BenchmarkForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BenchmarkForecast.NewBenchmarkForecast($"benchmark-forecast-{index}", "sample"));
        }
        var report = BenchmarkForecastReport.BuildBenchmarkForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BenchmarkForecastReportCountsEachRecordOnce();
    }
}
