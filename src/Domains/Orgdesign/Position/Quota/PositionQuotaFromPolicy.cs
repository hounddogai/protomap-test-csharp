namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Functions of position quota from policy.</summary>
public static class PositionQuotaFromPolicy
{
    public static Dictionary<string, double> SummarizePendingCostModelPoliciesForQuota(List<PendingCostModelPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<PositionQuota> DerivePositionQuotasFromPolicy(List<PendingCostModelPolicy> items)
    {
        var outValue = new List<PositionQuota>();
        foreach (var item in items)
        {
            var derived = PositionQuota.NewPositionQuota(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergePositionQuota(PositionQuota item)
    {
        return WeightedCostModelPolicyPolicy.NormalizeWeightedCostModelPolicyCode(item.Code);
    }
}
