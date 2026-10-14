namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FloorPlanRequestReportChecks
{
    public static void FloorPlanRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703005830);
        var items = new List<FloorPlanRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FloorPlanRequest.NewFloorPlanRequest($"floor-plan-request-{index}", "sample"));
        }
        var report = FloorPlanRequestReport.BuildFloorPlanRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FloorPlanRequestReportCountsEachRecordOnce();
    }
}
