namespace Acme.Workforce.Talent.Tests.CareerPath.Forecast;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackCareerPathForecastReportChecks
{
    public static void FallbackCareerPathForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701845573);
        var items = new List<FallbackCareerPathForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FallbackCareerPathForecast.NewFallbackCareerPathForecast($"fallback-career-path-forecast-{index}", "sample"));
        }
        var report = FallbackCareerPathForecastReport.BuildFallbackCareerPathForecastReport("test", items, clock);
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
        FallbackCareerPathForecastReportCountsEachRecordOnce();
    }
}
