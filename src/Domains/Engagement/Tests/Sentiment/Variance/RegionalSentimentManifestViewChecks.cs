namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalSentimentManifestViewChecks
{
    public static void RegionalSentimentManifestViewShowsTheStatusInCapitals()
    {
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("code-64", "name-23");
        var view = RegionalSentimentManifestView.ToRegionalSentimentManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalSentimentManifestView.ToRegionalSentimentManifestViews(new List<RegionalSentimentManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalSentimentManifestViewShowsTheStatusInCapitals();
    }
}
