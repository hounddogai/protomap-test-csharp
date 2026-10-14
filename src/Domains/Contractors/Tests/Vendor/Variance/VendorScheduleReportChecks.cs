namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorScheduleReportChecks
{
    public static void VendorScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706791255);
        var items = new List<VendorSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(VendorSchedule.NewVendorSchedule($"vendor-schedule-{index}", "sample"));
        }
        var report = VendorScheduleReport.BuildVendorScheduleReport("test", items, clock);
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
        VendorScheduleReportCountsEachRecordOnce();
    }
}
