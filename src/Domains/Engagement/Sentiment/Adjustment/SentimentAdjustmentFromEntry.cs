namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Functions of sentiment adjustment from entry.</summary>
public static class SentimentAdjustmentFromEntry
{
    public static Dictionary<string, double> SummarizeTownHallEntriesForAdjustment(List<TownHallEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<SentimentAdjustment> DeriveSentimentAdjustmentsFromEntry(List<TownHallEntry> items)
    {
        var outValue = new List<SentimentAdjustment>();
        foreach (var item in items)
        {
            var derived = SentimentAdjustment.NewSentimentAdjustment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifySentimentAdjustmentExtra(SentimentAdjustment item)
    {
        return TownHallDigestPolicy.TownHallDigestBucket(item.NetValue());
    }
}
