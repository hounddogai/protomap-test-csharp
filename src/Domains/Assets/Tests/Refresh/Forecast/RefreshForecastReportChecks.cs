namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RefreshForecastReportChecks
{
    public static void RefreshForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707252738);
        var items = new List<RefreshForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RefreshForecast.NewRefreshForecast($"refresh-forecast-{index}", "sample"));
        }
        var report = RefreshForecastReport.BuildRefreshForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RefreshForecastReportCountsEachRecordOnce();
    }
}
