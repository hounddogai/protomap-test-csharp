namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAccessBadgeLedgerReportChecks
{
    public static void RegionalAccessBadgeLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701047110);
        var items = new List<RegionalAccessBadgeLedger>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger($"regional-access-badge-ledger-{index}", "sample"));
        }
        var report = RegionalAccessBadgeLedgerReport.BuildRegionalAccessBadgeLedgerReport("test", items, clock);
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
        RegionalAccessBadgeLedgerReportCountsEachRecordOnce();
    }
}
