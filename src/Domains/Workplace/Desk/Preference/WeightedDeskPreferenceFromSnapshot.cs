namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Functions of weighted desk preference from snapshot.</summary>
public static class WeightedDeskPreferenceFromSnapshot
{
    public static Dictionary<string, double> SummarizeScheduledFloorPlanSnapshotsForPreference(List<ScheduledFloorPlanSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<WeightedDeskPreference> DeriveWeightedDeskPreferencesFromSnapshot(List<ScheduledFloorPlanSnapshot> items)
    {
        var outValue = new List<WeightedDeskPreference>();
        foreach (var item in items)
        {
            var derived = WeightedDeskPreference.NewWeightedDeskPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeWeightedDeskPreference(WeightedDeskPreference item)
    {
        return SeasonalFloorPlanBatchPolicy.NormalizeSeasonalFloorPlanBatchCode(item.Code);
    }
}
