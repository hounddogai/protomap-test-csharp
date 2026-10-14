namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictRateForecastReportChecks
{
    public static void StrictRateForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703819660);
        var items = new List<StrictRateForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictRateForecast.NewStrictRateForecast($"strict-rate-forecast-{index}", "sample"));
        }
        var report = StrictRateForecastReport.BuildStrictRateForecastReport("test", items, clock);
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
        StrictRateForecastReportCountsEachRecordOnce();
    }
}
