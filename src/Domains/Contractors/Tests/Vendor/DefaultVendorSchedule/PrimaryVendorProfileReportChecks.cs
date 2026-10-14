namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class PrimaryVendorProfileReportChecks
{
    public static void PrimaryVendorProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702607018);
        var items = new List<PrimaryVendorProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryVendorProfile.NewPrimaryVendorProfile($"primary-vendor-profile-{index}", "sample"));
        }
        var report = PrimaryVendorProfileReport.BuildPrimaryVendorProfileReport("test", items, clock);
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
        PrimaryVendorProfileReportCountsEachRecordOnce();
    }
}
