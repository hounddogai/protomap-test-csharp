namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Functions of action plan metric from summary.</summary>
public static class ActionPlanMetricFromSummary
{
    public static Dictionary<string, double> SummarizeTieredSentimentSummariesForMetric(List<TieredSentimentSummary> items)
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

    public static List<ActionPlanMetric> DeriveActionPlanMetricsFromSummary(List<TieredSentimentSummary> items)
    {
        var outValue = new List<ActionPlanMetric>();
        foreach (var item in items)
        {
            var derived = ActionPlanMetric.NewActionPlanMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignActionPlanMetric(ActionPlanMetric item)
    {
        return TieredSentimentSummaryPolicy.TieredSentimentSummaryBucket(item.MeasuredValue());
    }
}
