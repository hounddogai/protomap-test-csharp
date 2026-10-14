namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Orgdesign.Scenario.Reservation;

/// <summary>Functions of cost model mapping from milestone.</summary>
public static class CostModelMappingFromMilestone
{
    public static Dictionary<string, double> SummarizeScenarioMilestonesForMapping(List<ScenarioMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<CostModelMapping> DeriveCostModelMappingsFromMilestone(List<ScenarioMilestone> items)
    {
        var outValue = new List<CostModelMapping>();
        foreach (var item in items)
        {
            var derived = CostModelMapping.NewCostModelMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCostModelMapping(CostModelMapping item)
    {
        return ScenarioMilestonePolicy.ClassifyScenarioMilestone(item.ProjectedVolume());
    }
}
