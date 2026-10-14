namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class FloorPlanVarianceReportChecks
{
    public static void FloorPlanVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701183425);
        var items = new List<FloorPlanVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FloorPlanVariance.NewFloorPlanVariance($"floor-plan-variance-{index}", "sample"));
        }
        var report = FloorPlanVarianceReport.BuildFloorPlanVarianceReport("test", items, clock);
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
        FloorPlanVarianceReportCountsEachRecordOnce();
    }
}
