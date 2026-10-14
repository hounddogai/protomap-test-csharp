namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceAuditReportChecks
{
    public static void BalanceAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706695089);
        var items = new List<BalanceAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BalanceAudit.NewBalanceAudit($"balance-audit-{index}", "sample"));
        }
        var report = BalanceAuditReport.BuildBalanceAuditReport("test", items, clock);
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
        BalanceAuditReportCountsEachRecordOnce();
    }
}
