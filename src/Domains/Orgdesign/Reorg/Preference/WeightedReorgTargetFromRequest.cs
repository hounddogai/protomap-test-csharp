namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Functions of weighted reorg target from request.</summary>
public static class WeightedReorgTargetFromRequest
{
    public static Dictionary<string, double> SummarizeFallbackCostModelRequestsForTarget(List<FallbackCostModelRequest> items)
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

    public static List<WeightedReorgTarget> DeriveWeightedReorgTargetsFromRequest(List<FallbackCostModelRequest> items)
    {
        var outValue = new List<WeightedReorgTarget>();
        foreach (var item in items)
        {
            var derived = WeightedReorgTarget.NewWeightedReorgTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankWeightedReorgTarget(WeightedReorgTarget item)
    {
        return PendingCostModelRulePolicy.PendingCostModelRuleWeight(6, true);
    }
}
