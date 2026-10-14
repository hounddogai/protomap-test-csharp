namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class GlobalOccupancyForecastReportChecks
{
    public static void GlobalOccupancyForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706811760);
        var items = new List<GlobalOccupancyForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalOccupancyForecast.NewGlobalOccupancyForecast($"global-occupancy-forecast-{index}", "sample"));
        }
        var report = GlobalOccupancyForecastReport.BuildGlobalOccupancyForecastReport("test", items, clock);
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
        GlobalOccupancyForecastReportCountsEachRecordOnce();
    }
}
