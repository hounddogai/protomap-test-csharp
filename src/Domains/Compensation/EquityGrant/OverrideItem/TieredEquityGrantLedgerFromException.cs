namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Functions of tiered equity grant ledger from exception.</summary>
public static class TieredEquityGrantLedgerFromException
{
    public static Dictionary<string, double> SummarizePayBandExceptionsForLedger(List<PayBandException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<TieredEquityGrantLedger> DeriveTieredEquityGrantLedgersFromException(List<PayBandException> items)
    {
        var outValue = new List<TieredEquityGrantLedger>();
        foreach (var item in items)
        {
            var derived = TieredEquityGrantLedger.NewTieredEquityGrantLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredEquityGrantLedger(TieredEquityGrantLedger item)
    {
        return ScheduledPayBandSnapshotPolicy.ScheduledPayBandSnapshotBucket(item.EffectiveValue());
    }
}
