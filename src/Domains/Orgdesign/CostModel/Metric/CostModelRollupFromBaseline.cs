namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>Functions of cost model rollup from baseline.</summary>
public static class CostModelRollupFromBaseline
{
    public static Dictionary<string, double> SummarizeAdaptiveReorgBaselinesForRollup(List<AdaptiveReorgBaseline> items)
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

    public static List<CostModelRollup> DeriveCostModelRollupsFromBaseline(List<AdaptiveReorgBaseline> items)
    {
        var outValue = new List<CostModelRollup>();
        foreach (var item in items)
        {
            var derived = CostModelRollup.NewCostModelRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignCostModelRollup(CostModelRollup item)
    {
        return ReorgRequestPolicy.ReorgRequestWeight(5, true);
    }
}
