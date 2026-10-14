namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Functions of weighted cost model group from window.</summary>
public static class WeightedCostModelGroupFromWindow
{
    public static Dictionary<string, double> SummarizeManualScenarioWindowsForGroup(List<ManualScenarioWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<WeightedCostModelGroup> DeriveWeightedCostModelGroupsFromWindow(List<ManualScenarioWindow> items)
    {
        var outValue = new List<WeightedCostModelGroup>();
        foreach (var item in items)
        {
            var derived = WeightedCostModelGroup.NewWeightedCostModelGroup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyWeightedCostModelGroupExtra(WeightedCostModelGroup item)
    {
        return DefaultScenarioWindowPolicy.DefaultScenarioWindowBucket(item.NetTotal());
    }
}
