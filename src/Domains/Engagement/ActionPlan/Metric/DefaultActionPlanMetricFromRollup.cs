namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Functions of default action plan metric from rollup.</summary>
public static class DefaultActionPlanMetricFromRollup
{
    public static Dictionary<string, double> SummarizeSentimentRollupsForMetric(List<SentimentRollup> items)
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

    public static List<DefaultActionPlanMetric> DeriveDefaultActionPlanMetricsFromRollup(List<SentimentRollup> items)
    {
        var outValue = new List<DefaultActionPlanMetric>();
        foreach (var item in items)
        {
            var derived = DefaultActionPlanMetric.NewDefaultActionPlanMetric(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultActionPlanMetric(DefaultActionPlanMetric item)
    {
        return SentimentRollupPolicy.ClassifySentimentRollup(item.MeasuredValue());
    }
}
