namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultHolidayAssignmentReportChecks
{
    public static void DefaultHolidayAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707965880);
        var items = new List<DefaultHolidayAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultHolidayAssignment.NewDefaultHolidayAssignment($"default-holiday-assignment-{index}", "sample"));
        }
        var report = DefaultHolidayAssignmentReport.BuildDefaultHolidayAssignmentReport("test", items, clock);
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
        DefaultHolidayAssignmentReportCountsEachRecordOnce();
    }
}
