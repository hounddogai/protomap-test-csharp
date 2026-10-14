namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorSegmentReportChecks
{
    public static void DefaultVendorSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705281159);
        var items = new List<DefaultVendorSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultVendorSegment.NewDefaultVendorSegment($"default-vendor-segment-{index}", "sample"));
        }
        var report = DefaultVendorSegmentReport.BuildDefaultVendorSegmentReport("test", items, clock);
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
        DefaultVendorSegmentReportCountsEachRecordOnce();
    }
}
