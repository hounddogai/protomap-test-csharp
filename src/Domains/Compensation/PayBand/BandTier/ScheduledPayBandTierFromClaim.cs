namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Functions of scheduled pay band tier from claim.</summary>
public static class ScheduledPayBandTierFromClaim
{
    public static Dictionary<string, double> SummarizeBonusPoolClaimsForTier(List<BonusPoolClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<ScheduledPayBandTier> DeriveScheduledPayBandTiersFromClaim(List<BonusPoolClaim> items)
    {
        var outValue = new List<ScheduledPayBandTier>();
        foreach (var item in items)
        {
            var derived = ScheduledPayBandTier.NewScheduledPayBandTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyScheduledPayBandTier(ScheduledPayBandTier item)
    {
        return RollingBonusPoolProposalPolicy.RollingBonusPoolProposalBucket(item.WeightedVolume());
    }
}
