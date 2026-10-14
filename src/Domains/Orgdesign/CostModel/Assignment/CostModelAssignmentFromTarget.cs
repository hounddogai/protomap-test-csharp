namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Functions of cost model assignment from target.</summary>
public static class CostModelAssignmentFromTarget
{
    public static Dictionary<string, double> SummarizeReorgTargetsForAssignment(List<ReorgTarget> items)
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

    public static List<CostModelAssignment> DeriveCostModelAssignmentsFromTarget(List<ReorgTarget> items)
    {
        var outValue = new List<CostModelAssignment>();
        foreach (var item in items)
        {
            var derived = CostModelAssignment.NewCostModelAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateCostModelAssignment(CostModelAssignment item)
    {
        return ReorgTargetPolicy.ClassifyReorgTarget(item.EffectiveExposure());
    }
}
