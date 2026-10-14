namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of lenient sentiment estimate from manifest.</summary>
public static class LenientSentimentEstimateFromManifest
{
    public static Dictionary<string, double> SummarizeTownHallManifestsForEstimate(List<TownHallManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<LenientSentimentEstimate> DeriveLenientSentimentEstimatesFromManifest(List<TownHallManifest> items)
    {
        var outValue = new List<LenientSentimentEstimate>();
        foreach (var item in items)
        {
            var derived = LenientSentimentEstimate.NewLenientSentimentEstimate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateLenientSentimentEstimate(LenientSentimentEstimate item)
    {
        return WeightedTownHallManifestPolicy.NormalizeWeightedTownHallManifestCode(item.Code);
    }
}
