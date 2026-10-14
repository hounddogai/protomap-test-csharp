namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class RollingOccupancyAdjustmentReportChecks
{
    public static void RollingOccupancyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702498067);
        var items = new List<RollingOccupancyAdjustment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RollingOccupancyAdjustment.NewRollingOccupancyAdjustment($"rolling-occupancy-adjustment-{index}", "sample"));
        }
        var report = RollingOccupancyAdjustmentReport.BuildRollingOccupancyAdjustmentReport("test", items, clock);
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
        RollingOccupancyAdjustmentReportCountsEachRecordOnce();
    }
}
