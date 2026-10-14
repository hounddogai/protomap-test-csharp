namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayMilestoneReportChecks
{
    public static void HolidayMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707020926);
        var items = new List<HolidayMilestone>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HolidayMilestone.NewHolidayMilestone($"holiday-milestone-{index}", "sample"));
        }
        var report = HolidayMilestoneReport.BuildHolidayMilestoneReport("test", items, clock);
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
        HolidayMilestoneReportCountsEachRecordOnce();
    }
}
