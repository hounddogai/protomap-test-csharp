namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Functions of feedback claim from segment.</summary>
public static class FeedbackClaimFromSegment
{
    public static Dictionary<string, double> SummarizeActionPlanSegmentsForClaim(List<ActionPlanSegment> items)
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

    public static List<FeedbackClaim> DeriveFeedbackClaimsFromSegment(List<ActionPlanSegment> items)
    {
        var outValue = new List<FeedbackClaim>();
        foreach (var item in items)
        {
            var derived = FeedbackClaim.NewFeedbackClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateFeedbackClaim(FeedbackClaim item)
    {
        return ActionPlanMilestonePolicy.NormalizeActionPlanMilestoneCode(item.Code);
    }
}
