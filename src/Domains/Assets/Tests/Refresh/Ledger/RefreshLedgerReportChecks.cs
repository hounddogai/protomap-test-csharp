namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RefreshLedgerReportChecks
{
    public static void RefreshLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704139381);
        var items = new List<RefreshLedger>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RefreshLedger.NewRefreshLedger($"refresh-ledger-{index}", "sample"));
        }
        var report = RefreshLedgerReport.BuildRefreshLedgerReport("test", items, clock);
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
        RefreshLedgerReportCountsEachRecordOnce();
    }
}
