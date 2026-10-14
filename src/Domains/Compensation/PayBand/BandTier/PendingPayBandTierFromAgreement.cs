namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Functions of pending pay band tier from agreement.</summary>
public static class PendingPayBandTierFromAgreement
{
    public static Dictionary<string, double> SummarizeEquityGrantAgreementsForTier(List<EquityGrantAgreement> items)
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

    public static List<PendingPayBandTier> DerivePendingPayBandTiersFromAgreement(List<EquityGrantAgreement> items)
    {
        var outValue = new List<PendingPayBandTier>();
        foreach (var item in items)
        {
            var derived = PendingPayBandTier.NewPendingPayBandTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePendingPayBandTier(PendingPayBandTier item)
    {
        return EquityGrantExceptionPolicy.EquityGrantExceptionBucket(item.EffectiveValue());
    }
}
