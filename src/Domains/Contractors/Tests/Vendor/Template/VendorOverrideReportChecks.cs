namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorOverrideReportChecks
{
    public static void VendorOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708636133);
        var items = new List<VendorOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VendorOverride.NewVendorOverride($"vendor-override-{index}", "sample"));
        }
        var report = VendorOverrideReport.BuildVendorOverrideReport("test", items, clock);
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
        VendorOverrideReportCountsEachRecordOnce();
    }
}
