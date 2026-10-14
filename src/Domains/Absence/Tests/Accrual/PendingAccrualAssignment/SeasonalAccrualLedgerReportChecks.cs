namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class SeasonalAccrualLedgerReportChecks
{
    public static void SeasonalAccrualLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702237170);
        var items = new List<SeasonalAccrualLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalAccrualLedger.NewSeasonalAccrualLedger($"seasonal-accrual-ledger-{index}", "sample"));
        }
        var report = SeasonalAccrualLedgerReport.BuildSeasonalAccrualLedgerReport("test", items, clock);
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
        SeasonalAccrualLedgerReportCountsEachRecordOnce();
    }
}
