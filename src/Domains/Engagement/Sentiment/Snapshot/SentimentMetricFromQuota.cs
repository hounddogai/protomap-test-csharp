namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Functions of sentiment metric from quota.</summary>
public static class SentimentMetricFromQuota
{
    public static Dictionary<string, double> SummarizeActionPlanQuotasForMetric(List<ActionPlanQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<SentimentMetric> DeriveSentimentMetricsFromQuota(List<ActionPlanQuota> items)
    {
        var outValue = new List<SentimentMetric>();
        foreach (var item in items)
        {
            var derived = SentimentMetric.NewSentimentMetric(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankSentimentMetric(SentimentMetric item)
    {
        return ActionPlanEntryPolicy.ActionPlanEntryBucket(item.AdjustedLoad());
    }
}
