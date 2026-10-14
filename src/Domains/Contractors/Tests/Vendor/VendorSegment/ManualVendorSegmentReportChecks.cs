namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class ManualVendorSegmentReportChecks
{
    public static void ManualVendorSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702237123);
        var items = new List<ManualVendorSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualVendorSegment.NewManualVendorSegment($"manual-vendor-segment-{index}", "sample"));
        }
        var report = ManualVendorSegmentReport.BuildManualVendorSegmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ManualVendorSegmentReportCountsEachRecordOnce();
    }
}
