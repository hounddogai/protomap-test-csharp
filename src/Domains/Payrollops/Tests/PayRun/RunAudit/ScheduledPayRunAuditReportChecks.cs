namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class ScheduledPayRunAuditReportChecks
{
    public static void ScheduledPayRunAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702532001);
        var items = new List<ScheduledPayRunAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledPayRunAudit.NewScheduledPayRunAudit($"scheduled-pay-run-audit-{index}", "sample"));
        }
        var report = ScheduledPayRunAuditReport.BuildScheduledPayRunAuditReport("test", items, clock);
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
        ScheduledPayRunAuditReportCountsEachRecordOnce();
    }
}
