namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Functions of regional cost model assignment from bundle.</summary>
public static class RegionalCostModelAssignmentFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalScenarioBundlesForAssignment(List<GlobalScenarioBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RegionalCostModelAssignment> DeriveRegionalCostModelAssignmentsFromBundle(List<GlobalScenarioBundle> items)
    {
        var outValue = new List<RegionalCostModelAssignment>();
        foreach (var item in items)
        {
            var derived = RegionalCostModelAssignment.NewRegionalCostModelAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateRegionalCostModelAssignment(RegionalCostModelAssignment item)
    {
        return ScenarioTargetPolicy.ScenarioTargetBucket(item.NetValue());
    }
}
