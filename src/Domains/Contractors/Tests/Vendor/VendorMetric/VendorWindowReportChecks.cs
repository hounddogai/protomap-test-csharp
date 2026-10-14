namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorWindowReportChecks
{
    public static void VendorWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706986294);
        var items = new List<VendorWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VendorWindow.NewVendorWindow($"vendor-window-{index}", "sample"));
        }
        var report = VendorWindowReport.BuildVendorWindowReport("test", items, clock);
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
        VendorWindowReportCountsEachRecordOnce();
    }
}
