namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayReviewReportChecks
{
    public static void HolidayReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705822010);
        var items = new List<HolidayReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HolidayReview.NewHolidayReview($"holiday-review-{index}", "sample"));
        }
        var report = HolidayReviewReport.BuildHolidayReviewReport("test", items, clock);
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
        HolidayReviewReportCountsEachRecordOnce();
    }
}
