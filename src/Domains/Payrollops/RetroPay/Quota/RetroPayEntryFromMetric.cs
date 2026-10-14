namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Functions of retro pay entry from metric.</summary>
public static class RetroPayEntryFromMetric
{
    public static Dictionary<string, double> SummarizeAdaptiveRetroPayMetricsForEntry(List<AdaptiveRetroPayMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<RetroPayEntry> DeriveRetroPayEntriesFromMetric(List<AdaptiveRetroPayMetric> items)
    {
        var outValue = new List<RetroPayEntry>();
        foreach (var item in items)
        {
            var derived = RetroPayEntry.NewRetroPayEntry(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeRetroPayEntry(RetroPayEntry item)
    {
        return WeightedRetroPayClaimPolicy.WeightedRetroPayClaimBucket(item.AdjustedVolume());
    }
}
