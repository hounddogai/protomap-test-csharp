namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Orgdesign.Reorg.Baseline;

/// <summary>Functions of cost model checkpoint from budget.</summary>
public static class CostModelCheckpointFromBudget
{
    public static Dictionary<string, double> SummarizeReorgBudgetsForCheckpoint(List<ReorgBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<CostModelCheckpoint> DeriveCostModelCheckpointsFromBudget(List<ReorgBudget> items)
    {
        var outValue = new List<CostModelCheckpoint>();
        foreach (var item in items)
        {
            var derived = CostModelCheckpoint.NewCostModelCheckpoint(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }
}
