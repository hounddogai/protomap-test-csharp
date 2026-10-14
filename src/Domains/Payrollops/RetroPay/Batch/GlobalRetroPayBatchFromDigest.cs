namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Functions of global retro pay batch from digest.</summary>
public static class GlobalRetroPayBatchFromDigest
{
    public static Dictionary<string, double> SummarizeRetroPayDigestsForBatch(List<RetroPayDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<GlobalRetroPayBatch> DeriveGlobalRetroPayBatchesFromDigest(List<RetroPayDigest> items)
    {
        var outValue = new List<GlobalRetroPayBatch>();
        foreach (var item in items)
        {
            var derived = GlobalRetroPayBatch.NewGlobalRetroPayBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankGlobalRetroPayBatch(GlobalRetroPayBatch item)
    {
        return DefaultRetroPayAllocationPolicy.ClassifyDefaultRetroPayAllocation(item.ProjectedLoad());
    }
}
