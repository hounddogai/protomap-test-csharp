namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Functions of fallback action plan segment from preference.</summary>
public static class FallbackActionPlanSegmentFromPreference
{
    public static Dictionary<string, double> SummarizeAdaptiveFeedbackPreferencesForSegment(List<AdaptiveFeedbackPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<FallbackActionPlanSegment> DeriveFallbackActionPlanSegmentsFromPreference(List<AdaptiveFeedbackPreference> items)
    {
        var outValue = new List<FallbackActionPlanSegment>();
        foreach (var item in items)
        {
            var derived = FallbackActionPlanSegment.NewFallbackActionPlanSegment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveFallbackActionPlanSegment(FallbackActionPlanSegment item)
    {
        return FeedbackForecastPolicy.FeedbackForecastWeight(5, true);
    }
}
