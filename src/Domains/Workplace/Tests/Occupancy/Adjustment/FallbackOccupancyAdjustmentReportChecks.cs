namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackOccupancyAdjustmentReportChecks
{
    public static void FallbackOccupancyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706796314);
        var items = new List<FallbackOccupancyAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment($"fallback-occupancy-adjustment-{index}", "sample"));
        }
        var report = FallbackOccupancyAdjustmentReport.BuildFallbackOccupancyAdjustmentReport("test", items, clock);
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
        FallbackOccupancyAdjustmentReportCountsEachRecordOnce();
    }
}
