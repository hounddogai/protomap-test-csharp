namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Functions of tiered occupancy rule from quota.</summary>
public static class TieredOccupancyRuleFromQuota
{
    public static Dictionary<string, double> SummarizeFloorPlanQuotasForRule(List<FloorPlanQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<TieredOccupancyRule> DeriveTieredOccupancyRulesFromQuota(List<FloorPlanQuota> items)
    {
        var outValue = new List<TieredOccupancyRule>();
        foreach (var item in items)
        {
            var derived = TieredOccupancyRule.NewTieredOccupancyRule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeTieredOccupancyRule(TieredOccupancyRule item)
    {
        return ManualFloorPlanAssignmentPolicy.ManualFloorPlanAssignmentWeight(6, true);
    }
}
