namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledAuditPlanReportChecks
{
    public static void ScheduledAuditPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708203391);
        var items = new List<ScheduledAuditPlan>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledAuditPlan.NewScheduledAuditPlan($"scheduled-audit-plan-{index}", "sample"));
        }
        var report = ScheduledAuditPlanReport.BuildScheduledAuditPlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledAuditPlanReportCountsEachRecordOnce();
    }
}
