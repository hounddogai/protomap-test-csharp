namespace Acme.Workforce.Compliance.Tests.Audit.Cycle;

using Acme.Workforce.Compliance.Audit.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedAuditLedgerReportChecks
{
    public static void WeightedAuditLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706076686);
        var items = new List<WeightedAuditLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(WeightedAuditLedger.NewWeightedAuditLedger($"weighted-audit-ledger-{index}", "sample"));
        }
        var report = WeightedAuditLedgerReport.BuildWeightedAuditLedgerReport("test", items, clock);
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
        WeightedAuditLedgerReportCountsEachRecordOnce();
    }
}
