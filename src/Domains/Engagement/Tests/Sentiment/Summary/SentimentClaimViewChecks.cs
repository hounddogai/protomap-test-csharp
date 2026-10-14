namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentClaimViewChecks
{
    public static void SentimentClaimViewShowsTheStatusInCapitals()
    {
        var item = SentimentClaim.NewSentimentClaim("code-205", "title-722");
        var view = SentimentClaimView.ToSentimentClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentClaimView.ToSentimentClaimViews(new List<SentimentClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentClaimViewShowsTheStatusInCapitals();
    }
}
