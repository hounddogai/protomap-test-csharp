namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditPlanReportChecks
{
    public static void AuditPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708739570);
        var items = new List<AuditPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AuditPlan.NewAuditPlan($"audit-plan-{index}", "sample"));
        }
        var report = AuditPlanReport.BuildAuditPlanReport("test", items, clock);
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
        AuditPlanReportCountsEachRecordOnce();
    }
}
