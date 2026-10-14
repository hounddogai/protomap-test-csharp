namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class LenientSentimentManifestViewChecks
{
    public static void LenientSentimentManifestViewShowsTheStatusInCapitals()
    {
        var item = LenientSentimentManifest.NewLenientSentimentManifest("code-846", "title-24");
        var view = LenientSentimentManifestView.ToLenientSentimentManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSentimentManifestView.ToLenientSentimentManifestViews(new List<LenientSentimentManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSentimentManifestViewShowsTheStatusInCapitals();
    }
}
