namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Functions of policy allocation from threshold.</summary>
public static class PolicyAllocationFromThreshold
{
    public static Dictionary<string, double> SummarizeBalanceThresholdsForAllocation(List<BalanceThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<PolicyAllocation> DerivePolicyAllocationsFromThreshold(List<BalanceThreshold> items)
    {
        var outValue = new List<PolicyAllocation>();
        foreach (var item in items)
        {
            var derived = PolicyAllocation.NewPolicyAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPolicyAllocation(PolicyAllocation item)
    {
        return BalanceThresholdPolicy.ClassifyBalanceThreshold(item.ProjectedLoad());
    }
}
