namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackMetricViewChecks
{
    public static void FeedbackMetricViewShowsTheStatusInCapitals()
    {
        var item = FeedbackMetric.NewFeedbackMetric("code-872", "title-274");
        var view = FeedbackMetricView.ToFeedbackMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackMetricView.ToFeedbackMetricViews(new List<FeedbackMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackMetricViewShowsTheStatusInCapitals();
    }
}
