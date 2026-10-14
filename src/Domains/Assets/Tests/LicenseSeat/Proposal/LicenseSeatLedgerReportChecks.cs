namespace Acme.Workforce.Assets.Tests.LicenseSeat.Proposal;

using Acme.Workforce.Assets.LicenseSeat.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LicenseSeatLedgerReportChecks
{
    public static void LicenseSeatLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708496376);
        var items = new List<LicenseSeatLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LicenseSeatLedger.NewLicenseSeatLedger($"license-seat-ledger-{index}", "sample"));
        }
        var report = LicenseSeatLedgerReport.BuildLicenseSeatLedgerReport("test", items, clock);
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
        LicenseSeatLedgerReportCountsEachRecordOnce();
    }
}
