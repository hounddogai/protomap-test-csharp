namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Functions of pending garnishment tier from override.</summary>
public static class PendingGarnishmentTierFromOverride
{
    public static Dictionary<string, double> SummarizeRetroPayOverridesForTier(List<RetroPayOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<PendingGarnishmentTier> DerivePendingGarnishmentTiersFromOverride(List<RetroPayOverride> items)
    {
        var outValue = new List<PendingGarnishmentTier>();
        foreach (var item in items)
        {
            var derived = PendingGarnishmentTier.NewPendingGarnishmentTier(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolvePendingGarnishmentTier(PendingGarnishmentTier item)
    {
        return GlobalRetroPayBatchPolicy.GlobalRetroPayBatchWeight(3, true);
    }
}
