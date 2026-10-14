namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentPreferenceViewChecks
{
    public static void SentimentPreferenceViewShowsTheStatusInCapitals()
    {
        var item = SentimentPreference.NewSentimentPreference("code-911", "title-334");
        var view = SentimentPreferenceView.ToSentimentPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentPreferenceView.ToSentimentPreferenceViews(new List<SentimentPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentPreferenceViewShowsTheStatusInCapitals();
    }
}
