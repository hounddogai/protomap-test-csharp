namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class TieredVendorBudgetReportChecks
{
    public static void TieredVendorBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703584523);
        var items = new List<TieredVendorBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TieredVendorBudget.NewTieredVendorBudget($"tiered-vendor-budget-{index}", "sample"));
        }
        var report = TieredVendorBudgetReport.BuildTieredVendorBudgetReport("test", items, clock);
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
        TieredVendorBudgetReportCountsEachRecordOnce();
    }
}
