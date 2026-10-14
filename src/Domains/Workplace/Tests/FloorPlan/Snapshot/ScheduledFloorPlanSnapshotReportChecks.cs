namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledFloorPlanSnapshotReportChecks
{
    public static void ScheduledFloorPlanSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700637018);
        var items = new List<ScheduledFloorPlanSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot($"scheduled-floor-plan-snapshot-{index}", "sample"));
        }
        var report = ScheduledFloorPlanSnapshotReport.BuildScheduledFloorPlanSnapshotReport("test", items, clock);
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
        ScheduledFloorPlanSnapshotReportCountsEachRecordOnce();
    }
}
