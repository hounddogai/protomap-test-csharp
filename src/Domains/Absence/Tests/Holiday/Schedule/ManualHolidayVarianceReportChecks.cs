namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualHolidayVarianceReportChecks
{
    public static void ManualHolidayVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702007629);
        var items = new List<ManualHolidayVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualHolidayVariance.NewManualHolidayVariance($"manual-holiday-variance-{index}", "sample"));
        }
        var report = ManualHolidayVarianceReport.BuildManualHolidayVarianceReport("test", items, clock);
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
        ManualHolidayVarianceReportCountsEachRecordOnce();
    }
}
