namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Functions of scenario group from assignment.</summary>
public static class ScenarioGroupFromAssignment
{
    public static Dictionary<string, double> SummarizeCappedCostModelAssignmentsForGroup(List<CappedCostModelAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ScenarioGroup> DeriveScenarioGroupsFromAssignment(List<CappedCostModelAssignment> items)
    {
        var outValue = new List<ScenarioGroup>();
        foreach (var item in items)
        {
            var derived = ScenarioGroup.NewScenarioGroup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchScenarioGroup(ScenarioGroup item)
    {
        return CappedCostModelAssignmentPolicy.CappedCostModelAssignmentBucket(item.EffectiveExposure());
    }
}
