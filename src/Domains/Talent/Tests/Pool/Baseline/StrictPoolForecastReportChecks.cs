namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictPoolForecastReportChecks
{
    public static void StrictPoolForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705073271);
        var items = new List<StrictPoolForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictPoolForecast.NewStrictPoolForecast($"strict-pool-forecast-{index}", "sample"));
        }
        var report = StrictPoolForecastReport.BuildStrictPoolForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        StrictPoolForecastReportCountsEachRecordOnce();
    }
}
