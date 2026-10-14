namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentMappingViewChecks
{
    public static void SentimentMappingViewShowsTheStatusInCapitals()
    {
        var item = SentimentMapping.NewSentimentMapping("code-715", "title-167");
        var view = SentimentMappingView.ToSentimentMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentMappingView.ToSentimentMappingViews(new List<SentimentMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentMappingViewShowsTheStatusInCapitals();
    }
}
