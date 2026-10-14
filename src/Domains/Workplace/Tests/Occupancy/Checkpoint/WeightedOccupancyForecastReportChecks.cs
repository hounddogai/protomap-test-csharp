namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedOccupancyForecastReportChecks
{
    public static void WeightedOccupancyForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709199350);
        var items = new List<WeightedOccupancyForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedOccupancyForecast.NewWeightedOccupancyForecast($"weighted-occupancy-forecast-{index}", "sample"));
        }
        var report = WeightedOccupancyForecastReport.BuildWeightedOccupancyForecastReport("test", items, clock);
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
        WeightedOccupancyForecastReportCountsEachRecordOnce();
    }
}
