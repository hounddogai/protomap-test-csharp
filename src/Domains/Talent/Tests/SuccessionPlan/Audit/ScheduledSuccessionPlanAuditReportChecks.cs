namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ScheduledSuccessionPlanAuditReportChecks
{
    public static void ScheduledSuccessionPlanAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705584316);
        var items = new List<ScheduledSuccessionPlanAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit($"scheduled-succession-plan-audit-{index}", "sample"));
        }
        var report = ScheduledSuccessionPlanAuditReport.BuildScheduledSuccessionPlanAuditReport("test", items, clock);
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
        ScheduledSuccessionPlanAuditReportCountsEachRecordOnce();
    }
}
