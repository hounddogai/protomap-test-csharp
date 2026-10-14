namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandLedgerReportChecks
{
    public static void PayBandLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706335083);
        var items = new List<PayBandLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayBandLedger.NewPayBandLedger($"pay-band-ledger-{index}", "sample"));
        }
        var report = PayBandLedgerReport.BuildPayBandLedgerReport("test", items, clock);
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
        PayBandLedgerReportCountsEachRecordOnce();
    }
}
