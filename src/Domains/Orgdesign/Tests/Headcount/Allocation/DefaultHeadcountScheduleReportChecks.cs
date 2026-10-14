namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultHeadcountScheduleReportChecks
{
    public static void DefaultHeadcountScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701346989);
        var items = new List<DefaultHeadcountSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultHeadcountSchedule.NewDefaultHeadcountSchedule($"default-headcount-schedule-{index}", "sample"));
        }
        var report = DefaultHeadcountScheduleReport.BuildDefaultHeadcountScheduleReport("test", items, clock);
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
        DefaultHeadcountScheduleReportCountsEachRecordOnce();
    }
}
