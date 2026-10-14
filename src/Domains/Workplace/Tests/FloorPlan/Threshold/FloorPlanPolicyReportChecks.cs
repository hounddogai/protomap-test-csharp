namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanPolicyReportChecks
{
    public static void FloorPlanPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705988891);
        var items = new List<FloorPlanPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FloorPlanPolicy.NewFloorPlanPolicy($"floor-plan-policy-{index}", "sample"));
        }
        var report = FloorPlanPolicyReport.BuildFloorPlanPolicyReport("test", items, clock);
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
        FloorPlanPolicyReportCountsEachRecordOnce();
    }
}
