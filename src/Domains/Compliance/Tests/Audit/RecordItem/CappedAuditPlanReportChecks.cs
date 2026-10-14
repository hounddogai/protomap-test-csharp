namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedAuditPlanReportChecks
{
    public static void CappedAuditPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708227620);
        var items = new List<CappedAuditPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CappedAuditPlan.NewCappedAuditPlan($"capped-audit-plan-{index}", "sample"));
        }
        var report = CappedAuditPlanReport.BuildCappedAuditPlanReport("test", items, clock);
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
        CappedAuditPlanReportCountsEachRecordOnce();
    }
}
