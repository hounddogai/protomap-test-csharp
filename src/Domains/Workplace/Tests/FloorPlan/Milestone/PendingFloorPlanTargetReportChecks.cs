namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PendingFloorPlanTargetReportChecks
{
    public static void PendingFloorPlanTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700935718);
        var items = new List<PendingFloorPlanTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingFloorPlanTarget.NewPendingFloorPlanTarget($"pending-floor-plan-target-{index}", "sample"));
        }
        var report = PendingFloorPlanTargetReport.BuildPendingFloorPlanTargetReport("test", items, clock);
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
        PendingFloorPlanTargetReportCountsEachRecordOnce();
    }
}
