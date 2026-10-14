namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalWarrantyLedgerReportChecks
{
    public static void RegionalWarrantyLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709484358);
        var items = new List<RegionalWarrantyLedger>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalWarrantyLedger.NewRegionalWarrantyLedger($"regional-warranty-ledger-{index}", "sample"));
        }
        var report = RegionalWarrantyLedgerReport.BuildRegionalWarrantyLedgerReport("test", items, clock);
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
        RegionalWarrantyLedgerReportCountsEachRecordOnce();
    }
}
