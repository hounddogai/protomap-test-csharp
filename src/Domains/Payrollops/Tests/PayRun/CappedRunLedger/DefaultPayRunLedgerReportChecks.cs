namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunLedgerReportChecks
{
    public static void DefaultPayRunLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703464706);
        var items = new List<DefaultPayRunLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultPayRunLedger.NewDefaultPayRunLedger($"default-pay-run-ledger-{index}", "sample"));
        }
        var report = DefaultPayRunLedgerReport.BuildDefaultPayRunLedgerReport("test", items, clock);
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
        DefaultPayRunLedgerReportCountsEachRecordOnce();
    }
}
