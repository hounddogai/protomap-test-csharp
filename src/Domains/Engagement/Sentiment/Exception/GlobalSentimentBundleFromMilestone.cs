namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Functions of global sentiment bundle from milestone.</summary>
public static class GlobalSentimentBundleFromMilestone
{
    public static Dictionary<string, double> SummarizePendingFeedbackMilestonesForBundle(List<PendingFeedbackMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<GlobalSentimentBundle> DeriveGlobalSentimentBundlesFromMilestone(List<PendingFeedbackMilestone> items)
    {
        var outValue = new List<GlobalSentimentBundle>();
        foreach (var item in items)
        {
            var derived = GlobalSentimentBundle.NewGlobalSentimentBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreGlobalSentimentBundle(GlobalSentimentBundle item)
    {
        return PendingFeedbackMilestonePolicy.PendingFeedbackMilestoneWeight(2, true);
    }
}
