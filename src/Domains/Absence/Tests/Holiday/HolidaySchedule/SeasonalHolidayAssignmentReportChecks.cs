namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class SeasonalHolidayAssignmentReportChecks
{
    public static void SeasonalHolidayAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708028103);
        var items = new List<SeasonalHolidayAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalHolidayAssignment.NewSeasonalHolidayAssignment($"seasonal-holiday-assignment-{index}", "sample"));
        }
        var report = SeasonalHolidayAssignmentReport.BuildSeasonalHolidayAssignmentReport("test", items, clock);
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
        SeasonalHolidayAssignmentReportCountsEachRecordOnce();
    }
}
