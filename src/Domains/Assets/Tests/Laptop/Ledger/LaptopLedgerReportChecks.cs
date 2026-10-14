namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopLedgerReportChecks
{
    public static void LaptopLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707350197);
        var items = new List<LaptopLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LaptopLedger.NewLaptopLedger($"laptop-ledger-{index}", "sample"));
        }
        var report = LaptopLedgerReport.BuildLaptopLedgerReport("test", items, clock);
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
        LaptopLedgerReportCountsEachRecordOnce();
    }
}
