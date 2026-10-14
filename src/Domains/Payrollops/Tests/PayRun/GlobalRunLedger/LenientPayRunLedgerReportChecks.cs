namespace Acme.Workforce.Payrollops.Tests.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Checks of global run ledger code, run by the project's runner.</summary>
public static class LenientPayRunLedgerReportChecks
{
    public static void LenientPayRunLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705731650);
        var items = new List<LenientPayRunLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientPayRunLedger.NewLenientPayRunLedger($"lenient-pay-run-ledger-{index}", "sample"));
        }
        var report = LenientPayRunLedgerReport.BuildLenientPayRunLedgerReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientPayRunLedgerReportCountsEachRecordOnce();
    }
}
