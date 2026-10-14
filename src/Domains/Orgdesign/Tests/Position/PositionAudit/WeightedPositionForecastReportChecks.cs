namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class WeightedPositionForecastReportChecks
{
    public static void WeightedPositionForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706284527);
        var items = new List<WeightedPositionForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedPositionForecast.NewWeightedPositionForecast($"weighted-position-forecast-{index}", "sample"));
        }
        var report = WeightedPositionForecastReport.BuildWeightedPositionForecastReport("test", items, clock);
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
        WeightedPositionForecastReportCountsEachRecordOnce();
    }
}
