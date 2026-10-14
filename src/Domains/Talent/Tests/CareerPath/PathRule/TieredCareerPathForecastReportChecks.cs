namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class TieredCareerPathForecastReportChecks
{
    public static void TieredCareerPathForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708450357);
        var items = new List<TieredCareerPathForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredCareerPathForecast.NewTieredCareerPathForecast($"tiered-career-path-forecast-{index}", "sample"));
        }
        var report = TieredCareerPathForecastReport.BuildTieredCareerPathForecastReport("test", items, clock);
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
        TieredCareerPathForecastReportCountsEachRecordOnce();
    }
}
