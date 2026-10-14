namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Functions of legacy town hall checkpoint from preference.</summary>
public static class LegacyTownHallCheckpointFromPreference
{
    public static Dictionary<string, double> SummarizeDefaultActionPlanPreferencesForCheckpoint(List<DefaultActionPlanPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<LegacyTownHallCheckpoint> DeriveLegacyTownHallCheckpointsFromPreference(List<DefaultActionPlanPreference> items)
    {
        var outValue = new List<LegacyTownHallCheckpoint>();
        foreach (var item in items)
        {
            var derived = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLegacyTownHallCheckpoint(LegacyTownHallCheckpoint item)
    {
        return DefaultActionPlanPreferencePolicy.ClassifyDefaultActionPlanPreference(item.WeightedVolume());
    }
}
