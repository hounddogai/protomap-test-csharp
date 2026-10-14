namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Functions of feedback claim from milestone.</summary>
public static class FeedbackClaimFromMilestone
{
    public static Dictionary<string, double> SummarizeFeedbackMilestonesForClaim(List<FeedbackMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<FeedbackClaim> DeriveFeedbackClaimsFromMilestone(List<FeedbackMilestone> items)
    {
        var outValue = new List<FeedbackClaim>();
        foreach (var item in items)
        {
            var derived = FeedbackClaim.NewFeedbackClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeFeedbackClaim(FeedbackClaim item)
    {
        return FeedbackMilestonePolicy.FeedbackMilestoneWeight(4, true);
    }
}
