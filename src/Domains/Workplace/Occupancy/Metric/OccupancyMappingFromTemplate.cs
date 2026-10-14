namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Functions of occupancy mapping from template.</summary>
public static class OccupancyMappingFromTemplate
{
    public static Dictionary<string, double> SummarizeFloorPlanTemplatesForMapping(List<FloorPlanTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<OccupancyMapping> DeriveOccupancyMappingsFromTemplate(List<FloorPlanTemplate> items)
    {
        var outValue = new List<OccupancyMapping>();
        foreach (var item in items)
        {
            var derived = OccupancyMapping.NewOccupancyMapping(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyOccupancyMapping(OccupancyMapping item)
    {
        return FloorPlanRulePolicy.FloorPlanRuleWeight(9, true);
    }
}
