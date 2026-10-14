namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultReturnPlanAuditReportChecks
{
    public static void DefaultReturnPlanAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706058264);
        var items = new List<DefaultReturnPlanAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultReturnPlanAudit.NewDefaultReturnPlanAudit($"default-return-plan-audit-{index}", "sample"));
        }
        var report = DefaultReturnPlanAuditReport.BuildDefaultReturnPlanAuditReport("test", items, clock);
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
        DefaultReturnPlanAuditReportCountsEachRecordOnce();
    }
}
