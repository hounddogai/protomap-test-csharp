namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Functions of strict action plan threshold from batch.</summary>
public static class StrictActionPlanThresholdFromBatch
{
    public static Dictionary<string, double> SummarizeFeedbackBatchesForThreshold(List<FeedbackBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<StrictActionPlanThreshold> DeriveStrictActionPlanThresholdsFromBatch(List<FeedbackBatch> items)
    {
        var outValue = new List<StrictActionPlanThreshold>();
        foreach (var item in items)
        {
            var derived = StrictActionPlanThreshold.NewStrictActionPlanThreshold(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeStrictActionPlanThreshold(StrictActionPlanThreshold item)
    {
        return StrictFeedbackQuotaPolicy.ClassifyStrictFeedbackQuota(item.AdjustedExposure());
    }
}
