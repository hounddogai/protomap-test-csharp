namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyAdjustmentReportChecks
{
    public static void OccupancyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707120799);
        var items = new List<OccupancyAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(OccupancyAdjustment.NewOccupancyAdjustment($"occupancy-adjustment-{index}", "sample"));
        }
        var report = OccupancyAdjustmentReport.BuildOccupancyAdjustmentReport("test", items, clock);
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
        OccupancyAdjustmentReportCountsEachRecordOnce();
    }
}
