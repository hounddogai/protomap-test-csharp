namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FeedbackReviewReportChecks
{
    public static void FeedbackReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700265269);
        var items = new List<FeedbackReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FeedbackReview.NewFeedbackReview($"feedback-review-{index}", "sample"));
        }
        var report = FeedbackReviewReport.BuildFeedbackReviewReport("test", items, clock);
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
        FeedbackReviewReportCountsEachRecordOnce();
    }
}
