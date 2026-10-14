namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackWindowReportChecks
{
    public static void SeasonalFeedbackWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708887751);
        var items = new List<SeasonalFeedbackWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalFeedbackWindow.NewSeasonalFeedbackWindow($"seasonal-feedback-window-{index}", "sample"));
        }
        var report = SeasonalFeedbackWindowReport.BuildSeasonalFeedbackWindowReport("test", items, clock);
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
        SeasonalFeedbackWindowReportCountsEachRecordOnce();
    }
}
