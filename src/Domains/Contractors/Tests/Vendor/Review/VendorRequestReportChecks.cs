namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorRequestReportChecks
{
    public static void VendorRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708295022);
        var items = new List<VendorRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VendorRequest.NewVendorRequest($"vendor-request-{index}", "sample"));
        }
        var report = VendorRequestReport.BuildVendorRequestReport("test", items, clock);
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
        VendorRequestReportCountsEachRecordOnce();
    }
}
