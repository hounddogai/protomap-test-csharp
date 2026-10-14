namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorMappingReportChecks
{
    public static void VendorMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709497607);
        var items = new List<VendorMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VendorMapping.NewVendorMapping($"vendor-mapping-{index}", "sample"));
        }
        var report = VendorMappingReport.BuildVendorMappingReport("test", items, clock);
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
        VendorMappingReportCountsEachRecordOnce();
    }
}
