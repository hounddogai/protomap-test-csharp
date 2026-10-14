namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Functions of town hall override from mapping.</summary>
public static class TownHallOverrideFromMapping
{
    public static Dictionary<string, double> SummarizeActionPlanMappingsForOverride(List<ActionPlanMapping> items)
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

    public static List<TownHallOverride> DeriveTownHallOverridesFromMapping(List<ActionPlanMapping> items)
    {
        var outValue = new List<TownHallOverride>();
        foreach (var item in items)
        {
            var derived = TownHallOverride.NewTownHallOverride(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceTownHallOverride(TownHallOverride item)
    {
        return AdaptiveActionPlanMappingPolicy.ClassifyAdaptiveActionPlanMapping(item.NetVolume());
    }
}
