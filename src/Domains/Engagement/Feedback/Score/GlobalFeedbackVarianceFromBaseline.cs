namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Functions of global feedback variance from baseline.</summary>
public static class GlobalFeedbackVarianceFromBaseline
{
    public static Dictionary<string, double> SummarizePendingActionPlanBaselinesForVariance(List<PendingActionPlanBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<GlobalFeedbackVariance> DeriveGlobalFeedbackVariancesFromBaseline(List<PendingActionPlanBaseline> items)
    {
        var outValue = new List<GlobalFeedbackVariance>();
        foreach (var item in items)
        {
            var derived = GlobalFeedbackVariance.NewGlobalFeedbackVariance(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankGlobalFeedbackVariance(GlobalFeedbackVariance item)
    {
        return BatchedActionPlanPreferencePolicy.NormalizeBatchedActionPlanPreferenceCode(item.Code);
    }
}
