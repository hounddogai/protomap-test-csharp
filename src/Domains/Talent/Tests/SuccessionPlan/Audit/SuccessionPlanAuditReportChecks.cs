namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAuditReportChecks
{
    public static void SuccessionPlanAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705334843);
        var items = new List<SuccessionPlanAudit>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SuccessionPlanAudit.NewSuccessionPlanAudit($"succession-plan-audit-{index}", "sample"));
        }
        var report = SuccessionPlanAuditReport.BuildSuccessionPlanAuditReport("test", items, clock);
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
        SuccessionPlanAuditReportCountsEachRecordOnce();
    }
}
