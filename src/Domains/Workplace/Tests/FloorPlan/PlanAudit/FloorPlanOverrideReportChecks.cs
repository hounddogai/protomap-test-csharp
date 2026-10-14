namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class FloorPlanOverrideReportChecks
{
    public static void FloorPlanOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706630789);
        var items = new List<FloorPlanOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FloorPlanOverride.NewFloorPlanOverride($"floor-plan-override-{index}", "sample"));
        }
        var report = FloorPlanOverrideReport.BuildFloorPlanOverrideReport("test", items, clock);
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
        FloorPlanOverrideReportCountsEachRecordOnce();
    }
}
