namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class VendorProfileReportChecks
{
    public static void VendorProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706944609);
        var items = new List<VendorProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VendorProfile.NewVendorProfile($"vendor-profile-{index}", "sample"));
        }
        var report = VendorProfileReport.BuildVendorProfileReport("test", items, clock);
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
        VendorProfileReportCountsEachRecordOnce();
    }
}
