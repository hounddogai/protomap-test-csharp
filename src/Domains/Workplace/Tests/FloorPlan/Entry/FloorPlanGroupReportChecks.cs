namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanGroupReportChecks
{
    public static void FloorPlanGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700633882);
        var items = new List<FloorPlanGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FloorPlanGroup.NewFloorPlanGroup($"floor-plan-group-{index}", "sample"));
        }
        var report = FloorPlanGroupReport.BuildFloorPlanGroupReport("test", items, clock);
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
        FloorPlanGroupReportCountsEachRecordOnce();
    }
}
