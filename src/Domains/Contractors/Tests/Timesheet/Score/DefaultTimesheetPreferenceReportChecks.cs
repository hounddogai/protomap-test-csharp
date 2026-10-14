namespace Acme.Workforce.Contractors.Tests.Timesheet.Score;

using Acme.Workforce.Contractors.Timesheet.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultTimesheetPreferenceReportChecks
{
    public static void DefaultTimesheetPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709674177);
        var items = new List<DefaultTimesheetPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultTimesheetPreference.NewDefaultTimesheetPreference($"default-timesheet-preference-{index}", "sample"));
        }
        var report = DefaultTimesheetPreferenceReport.BuildDefaultTimesheetPreferenceReport("test", items, clock);
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
        DefaultTimesheetPreferenceReportCountsEachRecordOnce();
    }
}
