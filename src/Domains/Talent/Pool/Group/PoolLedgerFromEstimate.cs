namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Functions of pool ledger from estimate.</summary>
public static class PoolLedgerFromEstimate
{
    public static Dictionary<string, double> SummarizeNominationEstimatesForLedger(List<NominationEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<PoolLedger> DerivePoolLedgersFromEstimate(List<NominationEstimate> items)
    {
        var outValue = new List<PoolLedger>();
        foreach (var item in items)
        {
            var derived = PoolLedger.NewPoolLedger(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePoolLedger(PoolLedger item)
    {
        return NominationTargetPolicy.ClassifyNominationTarget(item.MeasuredValue());
    }
}
