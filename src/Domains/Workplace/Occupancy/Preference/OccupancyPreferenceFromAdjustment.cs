namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Functions of occupancy preference from adjustment.</summary>
public static class OccupancyPreferenceFromAdjustment
{
    public static Dictionary<string, double> SummarizeFloorPlanAdjustmentsForPreference(List<FloorPlanAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<OccupancyPreference> DeriveOccupancyPreferencesFromAdjustment(List<FloorPlanAdjustment> items)
    {
        var outValue = new List<OccupancyPreference>();
        foreach (var item in items)
        {
            var derived = OccupancyPreference.NewOccupancyPreference(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateOccupancyPreference(OccupancyPreference item)
    {
        return FloorPlanCheckpointPolicy.FloorPlanCheckpointWeight(1, true);
    }
}
