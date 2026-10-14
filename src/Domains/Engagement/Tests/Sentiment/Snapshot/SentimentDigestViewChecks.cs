namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentDigestViewChecks
{
    public static void SentimentDigestViewShowsTheStatusInCapitals()
    {
        var item = SentimentDigest.NewSentimentDigest("code-95", "title-637");
        var view = SentimentDigestView.ToSentimentDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentDigestView.ToSentimentDigestViews(new List<SentimentDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentDigestViewShowsTheStatusInCapitals();
    }
}
