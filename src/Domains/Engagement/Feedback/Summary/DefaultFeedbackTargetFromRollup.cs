namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Functions of default feedback target from rollup.</summary>
public static class DefaultFeedbackTargetFromRollup
{
    public static Dictionary<string, double> SummarizeDefaultActionPlanRollupsForTarget(List<DefaultActionPlanRollup> items)
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

    public static List<DefaultFeedbackTarget> DeriveDefaultFeedbackTargetsFromRollup(List<DefaultActionPlanRollup> items)
    {
        var outValue = new List<DefaultFeedbackTarget>();
        foreach (var item in items)
        {
            var derived = DefaultFeedbackTarget.NewDefaultFeedbackTarget(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanDefaultFeedbackTarget(DefaultFeedbackTarget item)
    {
        return ActionPlanClaimPolicy.ActionPlanClaimWeight(7, true);
    }
}
