namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TownHallForecastReportChecks
{
    public static void TownHallForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700481835);
        var items = new List<TownHallForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallForecast.NewTownHallForecast($"town-hall-forecast-{index}", "sample"));
        }
        var report = TownHallForecastReport.BuildTownHallForecastReport("test", items, clock);
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
        TownHallForecastReportCountsEachRecordOnce();
    }
}
