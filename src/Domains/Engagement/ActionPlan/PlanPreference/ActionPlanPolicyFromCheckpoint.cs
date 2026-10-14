namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Functions of action plan policy from checkpoint.</summary>
public static class ActionPlanPolicyFromCheckpoint
{
    public static Dictionary<string, double> SummarizeFeedbackCheckpointsForPolicy(List<FeedbackCheckpoint> items)
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

    public static List<ActionPlanPolicy> DeriveActionPlanPoliciesFromCheckpoint(List<FeedbackCheckpoint> items)
    {
        var outValue = new List<ActionPlanPolicy>();
        foreach (var item in items)
        {
            var derived = ActionPlanPolicy.NewActionPlanPolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyActionPlanPolicyExtra(ActionPlanPolicy item)
    {
        return FeedbackCheckpointPolicy.ClassifyFeedbackCheckpoint(item.AdjustedValue());
    }
}
