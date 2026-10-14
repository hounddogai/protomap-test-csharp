namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Functions of fallback sentiment plan from metric.</summary>
public static class FallbackSentimentPlanFromMetric
{
    public static Dictionary<string, double> SummarizeLegacyActionPlanMetricsForPlan(List<LegacyActionPlanMetric> items)
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

    public static List<FallbackSentimentPlan> DeriveFallbackSentimentPlansFromMetric(List<LegacyActionPlanMetric> items)
    {
        var outValue = new List<FallbackSentimentPlan>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentPlan.NewFallbackSentimentPlan(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyFallbackSentimentPlan(FallbackSentimentPlan item)
    {
        return LenientActionPlanBaselinePolicy.LenientActionPlanBaselineWeight(6, true);
    }
}
