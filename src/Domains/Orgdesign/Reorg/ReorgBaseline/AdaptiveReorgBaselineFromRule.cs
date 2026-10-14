namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Functions of adaptive reorg baseline from rule.</summary>
public static class AdaptiveReorgBaselineFromRule
{
    public static Dictionary<string, double> SummarizeCostModelRulesForBaseline(List<CostModelRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<AdaptiveReorgBaseline> DeriveAdaptiveReorgBaselinesFromRule(List<CostModelRule> items)
    {
        var outValue = new List<AdaptiveReorgBaseline>();
        foreach (var item in items)
        {
            var derived = AdaptiveReorgBaseline.NewAdaptiveReorgBaseline(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceAdaptiveReorgBaseline(AdaptiveReorgBaseline item)
    {
        return CostModelCheckpointPolicy.NormalizeCostModelCheckpointCode(item.Code);
    }
}
