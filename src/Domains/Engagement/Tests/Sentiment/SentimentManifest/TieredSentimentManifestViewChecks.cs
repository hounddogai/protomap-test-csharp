namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class TieredSentimentManifestViewChecks
{
    public static void TieredSentimentManifestViewShowsTheStatusInCapitals()
    {
        var item = TieredSentimentManifest.NewTieredSentimentManifest("code-148", "name-963");
        var view = TieredSentimentManifestView.ToTieredSentimentManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredSentimentManifestView.ToTieredSentimentManifestViews(new List<TieredSentimentManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredSentimentManifestViewShowsTheStatusInCapitals();
    }
}
