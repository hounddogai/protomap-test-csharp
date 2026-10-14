namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Functions of fallback sentiment bundle from tier.</summary>
public static class FallbackSentimentBundleFromTier
{
    public static Dictionary<string, double> SummarizeManualPulseSurveyTiersForBundle(List<ManualPulseSurveyTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<FallbackSentimentBundle> DeriveFallbackSentimentBundlesFromTier(List<ManualPulseSurveyTier> items)
    {
        var outValue = new List<FallbackSentimentBundle>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentBundle.NewFallbackSentimentBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanFallbackSentimentBundle(FallbackSentimentBundle item)
    {
        return PulseSurveyPlanPolicy.PulseSurveyPlanBucket(item.AdjustedVolume());
    }
}
