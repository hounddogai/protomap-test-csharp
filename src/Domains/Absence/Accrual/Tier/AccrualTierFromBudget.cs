namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Functions of accrual tier from budget.</summary>
public static class AccrualTierFromBudget
{
    public static Dictionary<string, double> SummarizePolicyBudgetsForTier(List<PolicyBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AccrualTier> DeriveAccrualTiersFromBudget(List<PolicyBudget> items)
    {
        var outValue = new List<AccrualTier>();
        foreach (var item in items)
        {
            var derived = AccrualTier.NewAccrualTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAccrualTier(AccrualTier item)
    {
        return PolicyOverridePolicy.PolicyOverrideBucket(item.WeightedLoad());
    }
}
