namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Functions of fallback sentiment bundle from review.</summary>
public static class FallbackSentimentBundleFromReview
{
    public static Dictionary<string, double> SummarizeAdaptiveActionPlanReviewsForBundle(List<AdaptiveActionPlanReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<FallbackSentimentBundle> DeriveFallbackSentimentBundlesFromReview(List<AdaptiveActionPlanReview> items)
    {
        var outValue = new List<FallbackSentimentBundle>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentBundle.NewFallbackSentimentBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyFallbackSentimentBundle(FallbackSentimentBundle item)
    {
        return CappedActionPlanDraftPolicy.NormalizeCappedActionPlanDraftCode(item.Code);
    }
}
