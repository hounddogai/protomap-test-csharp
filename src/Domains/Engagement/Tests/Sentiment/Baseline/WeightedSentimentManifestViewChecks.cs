namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedSentimentManifestViewChecks
{
    public static void WeightedSentimentManifestViewShowsTheStatusInCapitals()
    {
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("code-838", "title-174");
        var view = WeightedSentimentManifestView.ToWeightedSentimentManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedSentimentManifestView.ToWeightedSentimentManifestViews(new List<WeightedSentimentManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedSentimentManifestViewShowsTheStatusInCapitals();
    }
}
