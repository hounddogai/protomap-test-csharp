namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class OccupancyMetricReportChecks
{
    public static void OccupancyMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701291205);
        var items = new List<OccupancyMetric>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(OccupancyMetric.NewOccupancyMetric($"occupancy-metric-{index}", "sample"));
        }
        var report = OccupancyMetricReport.BuildOccupancyMetricReport("test", items, clock);
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
        OccupancyMetricReportCountsEachRecordOnce();
    }
}
