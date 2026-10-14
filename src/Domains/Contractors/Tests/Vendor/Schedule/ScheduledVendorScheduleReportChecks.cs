namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledVendorScheduleReportChecks
{
    public static void ScheduledVendorScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707768859);
        var items = new List<ScheduledVendorSchedule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledVendorSchedule.NewScheduledVendorSchedule($"scheduled-vendor-schedule-{index}", "sample"));
        }
        var report = ScheduledVendorScheduleReport.BuildScheduledVendorScheduleReport("test", items, clock);
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
        ScheduledVendorScheduleReportCountsEachRecordOnce();
    }
}
