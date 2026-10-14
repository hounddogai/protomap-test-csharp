namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Functions of regional town hall bundle from entry.</summary>
public static class RegionalTownHallBundleFromEntry
{
    public static Dictionary<string, double> SummarizeManualActionPlanEntriesForBundle(List<ManualActionPlanEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<RegionalTownHallBundle> DeriveRegionalTownHallBundlesFromEntry(List<ManualActionPlanEntry> items)
    {
        var outValue = new List<RegionalTownHallBundle>();
        foreach (var item in items)
        {
            var derived = RegionalTownHallBundle.NewRegionalTownHallBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRegionalTownHallBundle(RegionalTownHallBundle item)
    {
        return ManualActionPlanEntryPolicy.ClassifyManualActionPlanEntry(item.EffectiveValue());
    }
}
