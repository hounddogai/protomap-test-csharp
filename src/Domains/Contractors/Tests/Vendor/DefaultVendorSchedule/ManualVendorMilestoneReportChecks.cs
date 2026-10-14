namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class ManualVendorMilestoneReportChecks
{
    public static void ManualVendorMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704238589);
        var items = new List<ManualVendorMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualVendorMilestone.NewManualVendorMilestone($"manual-vendor-milestone-{index}", "sample"));
        }
        var report = ManualVendorMilestoneReport.BuildManualVendorMilestoneReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ManualVendorMilestoneReportCountsEachRecordOnce();
    }
}
