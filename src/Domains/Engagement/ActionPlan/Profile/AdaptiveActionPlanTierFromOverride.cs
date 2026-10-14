namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Functions of adaptive action plan tier from override.</summary>
public static class AdaptiveActionPlanTierFromOverride
{
    public static Dictionary<string, double> SummarizeScheduledTownHallOverridesForTier(List<ScheduledTownHallOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AdaptiveActionPlanTier> DeriveAdaptiveActionPlanTiersFromOverride(List<ScheduledTownHallOverride> items)
    {
        var outValue = new List<AdaptiveActionPlanTier>();
        foreach (var item in items)
        {
            var derived = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeAdaptiveActionPlanTier(AdaptiveActionPlanTier item)
    {
        return ScheduledTownHallOverridePolicy.ScheduledTownHallOverrideWeight(3, true);
    }
}
