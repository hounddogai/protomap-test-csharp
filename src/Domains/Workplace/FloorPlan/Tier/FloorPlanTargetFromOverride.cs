namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Functions of floor plan target from override.</summary>
public static class FloorPlanTargetFromOverride
{
    public static Dictionary<string, double> SummarizeDeskOverridesForTarget(List<DeskOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<FloorPlanTarget> DeriveFloorPlanTargetsFromOverride(List<DeskOverride> items)
    {
        var outValue = new List<FloorPlanTarget>();
        foreach (var item in items)
        {
            var derived = FloorPlanTarget.NewFloorPlanTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeFloorPlanTarget(FloorPlanTarget item)
    {
        return DeskMappingPolicy.DeskMappingWeight(7, true);
    }
}
