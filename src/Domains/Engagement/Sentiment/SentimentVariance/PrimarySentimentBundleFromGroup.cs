namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Feedback.FeedbackGroup;

/// <summary>Functions of primary sentiment bundle from group.</summary>
public static class PrimarySentimentBundleFromGroup
{
    public static Dictionary<string, double> SummarizeCappedFeedbackGroupsForBundle(List<CappedFeedbackGroup> items)
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

    public static List<PrimarySentimentBundle> DerivePrimarySentimentBundlesFromGroup(List<CappedFeedbackGroup> items)
    {
        var outValue = new List<PrimarySentimentBundle>();
        foreach (var item in items)
        {
            var derived = PrimarySentimentBundle.NewPrimarySentimentBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyPrimarySentimentBundleExtra(PrimarySentimentBundle item)
    {
        return CappedFeedbackGroupPolicy.CappedFeedbackGroupWeight(9, true);
    }
}
