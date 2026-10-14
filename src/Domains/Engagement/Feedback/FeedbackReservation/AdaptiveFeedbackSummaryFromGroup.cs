namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Functions of adaptive feedback summary from group.</summary>
public static class AdaptiveFeedbackSummaryFromGroup
{
    public static Dictionary<string, double> SummarizePulseSurveyGroupsForSummary(List<PulseSurveyGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AdaptiveFeedbackSummary> DeriveAdaptiveFeedbackSummariesFromGroup(List<PulseSurveyGroup> items)
    {
        var outValue = new List<AdaptiveFeedbackSummary>();
        foreach (var item in items)
        {
            var derived = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileAdaptiveFeedbackSummary(AdaptiveFeedbackSummary item)
    {
        return PulseSurveyQuotaPolicy.PulseSurveyQuotaWeight(4, true);
    }
}
