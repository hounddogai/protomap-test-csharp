namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Functions of cost model group from request.</summary>
public static class CostModelGroupFromRequest
{
    public static Dictionary<string, double> SummarizeScenarioRequestsForGroup(List<ScenarioRequest> items)
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

    public static List<CostModelGroup> DeriveCostModelGroupsFromRequest(List<ScenarioRequest> items)
    {
        var outValue = new List<CostModelGroup>();
        foreach (var item in items)
        {
            var derived = CostModelGroup.NewCostModelGroup(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCostModelGroup(CostModelGroup item)
    {
        return ScenarioBatchPolicy.ScenarioBatchBucket(item.NetValue());
    }
}
