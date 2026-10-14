namespace Acme.Workforce.Absence.Policy.Batch;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Functions of policy batch from adjustment.</summary>
public static class PolicyBatchFromAdjustment
{
    public static Dictionary<string, double> SummarizeLegacyBalanceAdjustmentsForBatch(List<LegacyBalanceAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<PolicyBatch> DerivePolicyBatchesFromAdjustment(List<LegacyBalanceAdjustment> items)
    {
        var outValue = new List<PolicyBatch>();
        foreach (var item in items)
        {
            var derived = PolicyBatch.NewPolicyBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputePolicyBatch(PolicyBatch item)
    {
        return LegacyBalanceAdjustmentPolicy.ClassifyLegacyBalanceAdjustment(item.WeightedExposure());
    }
}
