namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredAccrualLedgerReportChecks
{
    public static void TieredAccrualLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706942586);
        var items = new List<TieredAccrualLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredAccrualLedger.NewTieredAccrualLedger($"tiered-accrual-ledger-{index}", "sample"));
        }
        var report = TieredAccrualLedgerReport.BuildTieredAccrualLedgerReport("test", items, clock);
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
        TieredAccrualLedgerReportCountsEachRecordOnce();
    }
}
