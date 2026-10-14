namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>Functions of tiered sentiment manifest from bundle.</summary>
public static class TieredSentimentManifestFromBundle
{
    public static Dictionary<string, double> SummarizeScheduledRecognitionBundlesForManifest(List<ScheduledRecognitionBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<TieredSentimentManifest> DeriveTieredSentimentManifestsFromBundle(List<ScheduledRecognitionBundle> items)
    {
        var outValue = new List<TieredSentimentManifest>();
        foreach (var item in items)
        {
            var derived = TieredSentimentManifest.NewTieredSentimentManifest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeTieredSentimentManifest(TieredSentimentManifest item)
    {
        return RecognitionMilestonePolicy.NormalizeRecognitionMilestoneCode(item.Code);
    }
}
