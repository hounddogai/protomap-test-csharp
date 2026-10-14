namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Functions of weighted retro pay notice from cycle.</summary>
public static class WeightedRetroPayNoticeFromCycle
{
    public static Dictionary<string, double> SummarizeFallbackPayRunCyclesForNotice(List<FallbackPayRunCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<WeightedRetroPayNotice> DeriveWeightedRetroPayNoticesFromCycle(List<FallbackPayRunCycle> items)
    {
        var outValue = new List<WeightedRetroPayNotice>();
        foreach (var item in items)
        {
            var derived = WeightedRetroPayNotice.NewWeightedRetroPayNotice(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileWeightedRetroPayNotice(WeightedRetroPayNotice item)
    {
        return AdaptivePayRunAllocationPolicy.AdaptivePayRunAllocationBucket(item.MeasuredValue());
    }
}
