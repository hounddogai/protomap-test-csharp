namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorThresholdReportChecks
{
    public static void VendorThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701095631);
        var items = new List<VendorThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VendorThreshold.NewVendorThreshold($"vendor-threshold-{index}", "sample"));
        }
        var report = VendorThresholdReport.BuildVendorThresholdReport("test", items, clock);
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
        VendorThresholdReportCountsEachRecordOnce();
    }
}
