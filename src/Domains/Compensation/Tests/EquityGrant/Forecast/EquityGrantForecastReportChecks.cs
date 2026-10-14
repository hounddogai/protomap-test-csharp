namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantForecastReportChecks
{
    public static void EquityGrantForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701736959);
        var items = new List<EquityGrantForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EquityGrantForecast.NewEquityGrantForecast($"equity-grant-forecast-{index}", "sample"));
        }
        var report = EquityGrantForecastReport.BuildEquityGrantForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        EquityGrantForecastReportCountsEachRecordOnce();
    }
}
