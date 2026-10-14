namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Functions of sentiment target from snapshot.</summary>
public static class SentimentTargetFromSnapshot
{
    public static Dictionary<string, double> SummarizeSentimentSnapshotsForTarget(List<SentimentSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<SentimentTarget> DeriveSentimentTargetsFromSnapshot(List<SentimentSnapshot> items)
    {
        var outValue = new List<SentimentTarget>();
        foreach (var item in items)
        {
            var derived = SentimentTarget.NewSentimentTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitSentimentTarget(SentimentTarget item)
    {
        return SentimentEntryPolicy.NormalizeSentimentEntryCode(item.Code);
    }
}
