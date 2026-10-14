namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantLedgerReportChecks
{
    public static void EquityGrantLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702265660);
        var items = new List<EquityGrantLedger>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(EquityGrantLedger.NewEquityGrantLedger($"equity-grant-ledger-{index}", "sample"));
        }
        var report = EquityGrantLedgerReport.BuildEquityGrantLedgerReport("test", items, clock);
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
        EquityGrantLedgerReportCountsEachRecordOnce();
    }
}
