namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientEquityGrantLedgerReportChecks
{
    public static void LenientEquityGrantLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708025662);
        var items = new List<LenientEquityGrantLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientEquityGrantLedger.NewLenientEquityGrantLedger($"lenient-equity-grant-ledger-{index}", "sample"));
        }
        var report = LenientEquityGrantLedgerReport.BuildLenientEquityGrantLedgerReport("test", items, clock);
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
        LenientEquityGrantLedgerReportCountsEachRecordOnce();
    }
}
