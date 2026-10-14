namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackDeskLedgerReportChecks
{
    public static void FallbackDeskLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707720648);
        var items = new List<FallbackDeskLedger>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FallbackDeskLedger.NewFallbackDeskLedger($"fallback-desk-ledger-{index}", "sample"));
        }
        var report = FallbackDeskLedgerReport.BuildFallbackDeskLedgerReport("test", items, clock);
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
        FallbackDeskLedgerReportCountsEachRecordOnce();
    }
}
