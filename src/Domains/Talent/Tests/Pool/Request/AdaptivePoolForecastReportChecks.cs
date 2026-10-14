namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptivePoolForecastReportChecks
{
    public static void AdaptivePoolForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703474489);
        var items = new List<AdaptivePoolForecast>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptivePoolForecast.NewAdaptivePoolForecast($"adaptive-pool-forecast-{index}", "sample"));
        }
        var report = AdaptivePoolForecastReport.BuildAdaptivePoolForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptivePoolForecastReportCountsEachRecordOnce();
    }
}
