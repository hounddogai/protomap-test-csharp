namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Functions of rolling equity grant ledger from cycle.</summary>
public static class RollingEquityGrantLedgerFromCycle
{
    public static Dictionary<string, double> SummarizeSeasonalPayBandCyclesForLedger(List<SeasonalPayBandCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<RollingEquityGrantLedger> DeriveRollingEquityGrantLedgersFromCycle(List<SeasonalPayBandCycle> items)
    {
        var outValue = new List<RollingEquityGrantLedger>();
        foreach (var item in items)
        {
            var derived = RollingEquityGrantLedger.NewRollingEquityGrantLedger(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRollingEquityGrantLedger(RollingEquityGrantLedger item)
    {
        return CappedPayBandQuotaPolicy.CappedPayBandQuotaBucket(item.NetValue());
    }
}
