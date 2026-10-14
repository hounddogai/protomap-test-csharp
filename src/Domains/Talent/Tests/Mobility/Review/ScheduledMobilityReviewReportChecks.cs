namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityReviewReportChecks
{
    public static void ScheduledMobilityReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709544234);
        var items = new List<ScheduledMobilityReview>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledMobilityReview.NewScheduledMobilityReview($"scheduled-mobility-review-{index}", "sample"));
        }
        var report = ScheduledMobilityReviewReport.BuildScheduledMobilityReviewReport("test", items, clock);
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
        ScheduledMobilityReviewReportCountsEachRecordOnce();
    }
}
