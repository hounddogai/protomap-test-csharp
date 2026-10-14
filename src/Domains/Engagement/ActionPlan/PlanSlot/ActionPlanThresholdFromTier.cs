namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Functions of action plan threshold from tier.</summary>
public static class ActionPlanThresholdFromTier
{
    public static Dictionary<string, double> SummarizeActionPlanTiersForThreshold(List<ActionPlanTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<ActionPlanThreshold> DeriveActionPlanThresholdsFromTier(List<ActionPlanTier> items)
    {
        var outValue = new List<ActionPlanThreshold>();
        foreach (var item in items)
        {
            var derived = ActionPlanThreshold.NewActionPlanThreshold(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceActionPlanThreshold(ActionPlanThreshold item)
    {
        return ActionPlanTierPolicy.ActionPlanTierBucket(item.AdjustedVolume());
    }
}
