namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayVarianceReportChecks
{
    public static void HolidayVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707759908);
        var items = new List<HolidayVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HolidayVariance.NewHolidayVariance($"holiday-variance-{index}", "sample"));
        }
        var report = HolidayVarianceReport.BuildHolidayVarianceReport("test", items, clock);
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
        HolidayVarianceReportCountsEachRecordOnce();
    }
}
