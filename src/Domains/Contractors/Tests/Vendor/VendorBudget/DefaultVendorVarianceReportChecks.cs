namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class DefaultVendorVarianceReportChecks
{
    public static void DefaultVendorVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702634254);
        var items = new List<DefaultVendorVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultVendorVariance.NewDefaultVendorVariance($"default-vendor-variance-{index}", "sample"));
        }
        var report = DefaultVendorVarianceReport.BuildDefaultVendorVarianceReport("test", items, clock);
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
        DefaultVendorVarianceReportCountsEachRecordOnce();
    }
}
