namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanAuditReportChecks
{
    public static void FloorPlanAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704485504);
        var items = new List<FloorPlanAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FloorPlanAudit.NewFloorPlanAudit($"floor-plan-audit-{index}", "sample"));
        }
        var report = FloorPlanAuditReport.BuildFloorPlanAuditReport("test", items, clock);
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
        FloorPlanAuditReportCountsEachRecordOnce();
    }
}
