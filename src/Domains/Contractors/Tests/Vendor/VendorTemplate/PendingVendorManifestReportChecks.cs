namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class PendingVendorManifestReportChecks
{
    public static void PendingVendorManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706877907);
        var items = new List<PendingVendorManifest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingVendorManifest.NewPendingVendorManifest($"pending-vendor-manifest-{index}", "sample"));
        }
        var report = PendingVendorManifestReport.BuildPendingVendorManifestReport("test", items, clock);
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
        PendingVendorManifestReportCountsEachRecordOnce();
    }
}
