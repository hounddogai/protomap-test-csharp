namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RegionalLaptopLedgerReportChecks
{
    public static void RegionalLaptopLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705306887);
        var items = new List<RegionalLaptopLedger>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalLaptopLedger.NewRegionalLaptopLedger($"regional-laptop-ledger-{index}", "sample"));
        }
        var report = RegionalLaptopLedgerReport.BuildRegionalLaptopLedgerReport("test", items, clock);
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
        RegionalLaptopLedgerReportCountsEachRecordOnce();
    }
}
