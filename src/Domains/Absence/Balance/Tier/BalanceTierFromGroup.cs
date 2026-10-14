namespace Acme.Workforce.Absence.Balance.Tier;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Functions of balance tier from group.</summary>
public static class BalanceTierFromGroup
{
    public static Dictionary<string, double> SummarizeAccrualGroupsForTier(List<AccrualGroup> items)
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

    public static List<BalanceTier> DeriveBalanceTiersFromGroup(List<AccrualGroup> items)
    {
        var outValue = new List<BalanceTier>();
        foreach (var item in items)
        {
            var derived = BalanceTier.NewBalanceTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeBalanceTier(BalanceTier item)
    {
        return AccrualGroupPolicy.AccrualGroupBucket(item.AdjustedLoad());
    }
}
