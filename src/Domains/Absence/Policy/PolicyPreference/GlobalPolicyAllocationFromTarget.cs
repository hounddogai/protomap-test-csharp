namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Functions of global policy allocation from target.</summary>
public static class GlobalPolicyAllocationFromTarget
{
    public static Dictionary<string, double> SummarizeRegionalBalanceTargetsForAllocation(List<RegionalBalanceTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<GlobalPolicyAllocation> DeriveGlobalPolicyAllocationsFromTarget(List<RegionalBalanceTarget> items)
    {
        var outValue = new List<GlobalPolicyAllocation>();
        foreach (var item in items)
        {
            var derived = GlobalPolicyAllocation.NewGlobalPolicyAllocation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyGlobalPolicyAllocation(GlobalPolicyAllocation item)
    {
        return ManualBalanceSummaryPolicy.NormalizeManualBalanceSummaryCode(item.Code);
    }
}
