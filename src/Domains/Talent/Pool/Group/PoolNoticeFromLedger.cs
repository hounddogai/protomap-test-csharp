namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Functions of pool notice from ledger.</summary>
public static class PoolNoticeFromLedger
{
    public static Dictionary<string, double> SummarizeCareerPathLedgersForNotice(List<CareerPathLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<PoolNotice> DerivePoolNoticesFromLedger(List<CareerPathLedger> items)
    {
        var outValue = new List<PoolNotice>();
        foreach (var item in items)
        {
            var derived = PoolNotice.NewPoolNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignPoolNotice(PoolNotice item)
    {
        return CareerPathLedgerPolicy.CareerPathLedgerWeight(2, true);
    }
}
