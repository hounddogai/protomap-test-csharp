namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Functions of pool assignment from tier.</summary>
public static class PoolAssignmentFromTier
{
    public static Dictionary<string, double> SummarizeNominationTiersForAssignment(List<NominationTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<PoolAssignment> DerivePoolAssignmentsFromTier(List<NominationTier> items)
    {
        var outValue = new List<PoolAssignment>();
        foreach (var item in items)
        {
            var derived = PoolAssignment.NewPoolAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DerivePoolAssignment(PoolAssignment item)
    {
        return NominationLedgerPolicy.NominationLedgerWeight(4, true);
    }
}
