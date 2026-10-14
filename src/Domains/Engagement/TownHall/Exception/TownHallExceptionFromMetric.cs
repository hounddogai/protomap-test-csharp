namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Functions of town hall exception from metric.</summary>
public static class TownHallExceptionFromMetric
{
    public static Dictionary<string, double> SummarizeSentimentMetricsForException(List<SentimentMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TownHallException> DeriveTownHallExceptionsFromMetric(List<SentimentMetric> items)
    {
        var outValue = new List<TownHallException>();
        foreach (var item in items)
        {
            var derived = TownHallException.NewTownHallException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectTownHallException(TownHallException item)
    {
        return SentimentEntryPolicy.SentimentEntryBucket(item.WeightedVolume());
    }
}
