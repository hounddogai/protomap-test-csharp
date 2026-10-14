namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Functions of cost model draft from assignment.</summary>
public static class CostModelDraftFromAssignment
{
    public static Dictionary<string, double> SummarizeScenarioAssignmentsForDraft(List<ScenarioAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<CostModelDraft> DeriveCostModelDraftsFromAssignment(List<ScenarioAssignment> items)
    {
        var outValue = new List<CostModelDraft>();
        foreach (var item in items)
        {
            var derived = CostModelDraft.NewCostModelDraft(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCostModelDraft(CostModelDraft item)
    {
        return ScenarioAssignmentPolicy.ClassifyScenarioAssignment(item.ProjectedTotal());
    }
}
