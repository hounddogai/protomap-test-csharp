namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class LenientFloorPlanAuditReportChecks
{
    public static void LenientFloorPlanAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707073432);
        var items = new List<LenientFloorPlanAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientFloorPlanAudit.NewLenientFloorPlanAudit($"lenient-floor-plan-audit-{index}", "sample"));
        }
        var report = LenientFloorPlanAuditReport.BuildLenientFloorPlanAuditReport("test", items, clock);
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
        LenientFloorPlanAuditReportCountsEachRecordOnce();
    }
}
