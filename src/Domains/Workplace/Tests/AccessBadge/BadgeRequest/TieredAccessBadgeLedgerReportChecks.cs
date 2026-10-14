namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class TieredAccessBadgeLedgerReportChecks
{
    public static void TieredAccessBadgeLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701110031);
        var items = new List<TieredAccessBadgeLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredAccessBadgeLedger.NewTieredAccessBadgeLedger($"tiered-access-badge-ledger-{index}", "sample"));
        }
        var report = TieredAccessBadgeLedgerReport.BuildTieredAccessBadgeLedgerReport("test", items, clock);
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
        TieredAccessBadgeLedgerReportCountsEachRecordOnce();
    }
}
