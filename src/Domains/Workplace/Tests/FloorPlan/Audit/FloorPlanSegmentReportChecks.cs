namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanSegmentReportChecks
{
    public static void FloorPlanSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707114398);
        var items = new List<FloorPlanSegment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FloorPlanSegment.NewFloorPlanSegment($"floor-plan-segment-{index}", "sample"));
        }
        var report = FloorPlanSegmentReport.BuildFloorPlanSegmentReport("test", items, clock);
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
        FloorPlanSegmentReportCountsEachRecordOnce();
    }
}
