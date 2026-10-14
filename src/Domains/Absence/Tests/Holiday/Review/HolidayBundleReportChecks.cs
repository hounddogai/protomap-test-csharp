namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidayBundleReportChecks
{
    public static void HolidayBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702311360);
        var items = new List<HolidayBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HolidayBundle.NewHolidayBundle($"holiday-bundle-{index}", "sample"));
        }
        var report = HolidayBundleReport.BuildHolidayBundleReport("test", items, clock);
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
        HolidayBundleReportCountsEachRecordOnce();
    }
}
