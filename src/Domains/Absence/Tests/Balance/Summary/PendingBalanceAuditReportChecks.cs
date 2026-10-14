namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingBalanceAuditReportChecks
{
    public static void PendingBalanceAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709766042);
        var items = new List<PendingBalanceAudit>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingBalanceAudit.NewPendingBalanceAudit($"pending-balance-audit-{index}", "sample"));
        }
        var report = PendingBalanceAuditReport.BuildPendingBalanceAuditReport("test", items, clock);
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
        PendingBalanceAuditReportCountsEachRecordOnce();
    }
}
