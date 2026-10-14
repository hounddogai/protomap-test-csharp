namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Functions of feedback estimate from bundle.</summary>
public static class FeedbackEstimateFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalSentimentBundlesForEstimate(List<GlobalSentimentBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<FeedbackEstimate> DeriveFeedbackEstimatesFromBundle(List<GlobalSentimentBundle> items)
    {
        var outValue = new List<FeedbackEstimate>();
        foreach (var item in items)
        {
            var derived = FeedbackEstimate.NewFeedbackEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateFeedbackEstimate(FeedbackEstimate item)
    {
        return RegionalSentimentSnapshotPolicy.ClassifyRegionalSentimentSnapshot(item.MeasuredValue());
    }
}
