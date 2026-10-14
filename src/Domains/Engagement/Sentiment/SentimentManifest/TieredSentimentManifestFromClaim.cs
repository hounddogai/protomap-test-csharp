namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Functions of tiered sentiment manifest from claim.</summary>
public static class TieredSentimentManifestFromClaim
{
    public static Dictionary<string, double> SummarizeLenientPulseSurveyClaimsForManifest(List<LenientPulseSurveyClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<TieredSentimentManifest> DeriveTieredSentimentManifestsFromClaim(List<LenientPulseSurveyClaim> items)
    {
        var outValue = new List<TieredSentimentManifest>();
        foreach (var item in items)
        {
            var derived = TieredSentimentManifest.NewTieredSentimentManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanTieredSentimentManifest(TieredSentimentManifest item)
    {
        return LenientPulseSurveyClaimPolicy.LenientPulseSurveyClaimWeight(6, true);
    }
}
