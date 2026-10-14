namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Functions of fallback sentiment profile from request.</summary>
public static class FallbackSentimentProfileFromRequest
{
    public static Dictionary<string, double> SummarizePulseSurveyRequestsForProfile(List<PulseSurveyRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<FallbackSentimentProfile> DeriveFallbackSentimentProfilesFromRequest(List<PulseSurveyRequest> items)
    {
        var outValue = new List<FallbackSentimentProfile>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentProfile.NewFallbackSentimentProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectFallbackSentimentProfile(FallbackSentimentProfile item)
    {
        return LenientPulseSurveyManifestPolicy.LenientPulseSurveyManifestWeight(2, true);
    }
}
