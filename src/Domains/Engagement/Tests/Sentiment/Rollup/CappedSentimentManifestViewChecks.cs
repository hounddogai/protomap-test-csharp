namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedSentimentManifestViewChecks
{
    public static void CappedSentimentManifestViewShowsTheStatusInCapitals()
    {
        var item = CappedSentimentManifest.NewCappedSentimentManifest("code-834", "title-310");
        var view = CappedSentimentManifestView.ToCappedSentimentManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedSentimentManifestView.ToCappedSentimentManifestViews(new List<CappedSentimentManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedSentimentManifestViewShowsTheStatusInCapitals();
    }
}
