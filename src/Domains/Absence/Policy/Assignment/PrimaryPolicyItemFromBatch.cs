namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Functions of primary policy item from batch.</summary>
public static class PrimaryPolicyItemFromBatch
{
    public static Dictionary<string, double> SummarizeLenientBalanceBatchesForItem(List<LenientBalanceBatch> items)
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

    public static List<PrimaryPolicyItem> DerivePrimaryPolicyItemsFromBatch(List<LenientBalanceBatch> items)
    {
        var outValue = new List<PrimaryPolicyItem>();
        foreach (var item in items)
        {
            var derived = PrimaryPolicyItem.NewPrimaryPolicyItem(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePrimaryPolicyItem(PrimaryPolicyItem item)
    {
        return LenientBalanceGroupPolicy.NormalizeLenientBalanceGroupCode(item.Code);
    }
}
