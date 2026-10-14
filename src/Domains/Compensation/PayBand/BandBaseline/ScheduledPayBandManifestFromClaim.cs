namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>Functions of scheduled pay band manifest from claim.</summary>
public static class ScheduledPayBandManifestFromClaim
{
    public static Dictionary<string, double> SummarizeBatchedEquityGrantClaimsForManifest(List<BatchedEquityGrantClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<ScheduledPayBandManifest> DeriveScheduledPayBandManifestsFromClaim(List<BatchedEquityGrantClaim> items)
    {
        var outValue = new List<ScheduledPayBandManifest>();
        foreach (var item in items)
        {
            var derived = ScheduledPayBandManifest.NewScheduledPayBandManifest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileScheduledPayBandManifest(ScheduledPayBandManifest item)
    {
        return ManualEquityGrantDraftPolicy.ManualEquityGrantDraftBucket(item.NetTotal());
    }
}
