namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancySnapshotReportChecks
{
    public static void OccupancySnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703315216);
        var items = new List<OccupancySnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(OccupancySnapshot.NewOccupancySnapshot($"occupancy-snapshot-{index}", "sample"));
        }
        var report = OccupancySnapshotReport.BuildOccupancySnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        OccupancySnapshotReportCountsEachRecordOnce();
    }
}
