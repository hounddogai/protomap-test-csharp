namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Functions of tiered bonus pool batch from policy.</summary>
public static class TieredBonusPoolBatchFromPolicy
{
    public static Dictionary<string, double> SummarizeLenientBonusPoolPoliciesForBatch(List<LenientBonusPoolPolicy> items)
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

    public static List<TieredBonusPoolBatch> DeriveTieredBonusPoolBatchesFromPolicy(List<LenientBonusPoolPolicy> items)
    {
        var outValue = new List<TieredBonusPoolBatch>();
        foreach (var item in items)
        {
            var derived = TieredBonusPoolBatch.NewTieredBonusPoolBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveTieredBonusPoolBatch(TieredBonusPoolBatch item)
    {
        return LenientBonusPoolPolicyPolicy.ClassifyLenientBonusPoolPolicy(item.MeasuredValue());
    }
}
