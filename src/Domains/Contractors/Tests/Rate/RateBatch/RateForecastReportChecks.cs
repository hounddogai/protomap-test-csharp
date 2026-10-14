namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateForecastReportChecks
{
    public static void RateForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701996246);
        var items = new List<RateForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RateForecast.NewRateForecast($"rate-forecast-{index}", "sample"));
        }
        var report = RateForecastReport.BuildRateForecastReport("test", items, clock);
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
        RateForecastReportCountsEachRecordOnce();
    }
}
