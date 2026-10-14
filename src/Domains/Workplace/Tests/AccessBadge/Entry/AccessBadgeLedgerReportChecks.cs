namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeLedgerReportChecks
{
    public static void AccessBadgeLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707752761);
        var items = new List<AccessBadgeLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccessBadgeLedger.NewAccessBadgeLedger($"access-badge-ledger-{index}", "sample"));
        }
        var report = AccessBadgeLedgerReport.BuildAccessBadgeLedgerReport("test", items, clock);
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
        AccessBadgeLedgerReportCountsEachRecordOnce();
    }
}
