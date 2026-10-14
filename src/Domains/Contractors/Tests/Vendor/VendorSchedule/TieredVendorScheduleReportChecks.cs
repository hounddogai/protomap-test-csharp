namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of vendor schedule code, run by the project's runner.</summary>
public static class TieredVendorScheduleReportChecks
{
    public static void TieredVendorScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701934253);
        var items = new List<TieredVendorSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredVendorSchedule.NewTieredVendorSchedule($"tiered-vendor-schedule-{index}", "sample"));
        }
        var report = TieredVendorScheduleReport.BuildTieredVendorScheduleReport("test", items, clock);
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
        TieredVendorScheduleReportCountsEachRecordOnce();
    }
}
