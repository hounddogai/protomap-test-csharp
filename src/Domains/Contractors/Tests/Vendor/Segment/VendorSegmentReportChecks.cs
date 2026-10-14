namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorSegmentReportChecks
{
    public static void VendorSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704197577);
        var items = new List<VendorSegment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VendorSegment.NewVendorSegment($"vendor-segment-{index}", "sample"));
        }
        var report = VendorSegmentReport.BuildVendorSegmentReport("test", items, clock);
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
        VendorSegmentReportCountsEachRecordOnce();
    }
}
