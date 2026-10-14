namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Functions of feedback metric from quota.</summary>
public static class FeedbackMetricFromQuota
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

    public static List<FeedbackMetric> DeriveFeedbackMetricsFromQuota(List<ActionPlanQuota> items)
    {
        var outValue = new List<FeedbackMetric>();
        foreach (var item in items)
        {
            var derived = FeedbackMetric.NewFeedbackMetric(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleFeedbackMetric(FeedbackMetric item)
    {
        return ActionPlanQuotaPolicy.ActionPlanQuotaBucket(item.MeasuredValue());
    }
}
