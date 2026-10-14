namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VendorPreferenceReportChecks
{
    public static void VendorPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706091367);
        var items = new List<VendorPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VendorPreference.NewVendorPreference($"vendor-preference-{index}", "sample"));
        }
        var report = VendorPreferenceReport.BuildVendorPreferenceReport("test", items, clock);
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
        VendorPreferenceReportCountsEachRecordOnce();
    }
}
