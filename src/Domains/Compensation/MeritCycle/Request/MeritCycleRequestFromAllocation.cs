namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Functions of merit cycle request from allocation.</summary>
public static class MeritCycleRequestFromAllocation
{
    public static Dictionary<string, double> SummarizeDefaultPayBandAllocationsForRequest(List<DefaultPayBandAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<MeritCycleRequest> DeriveMeritCycleRequestsFromAllocation(List<DefaultPayBandAllocation> items)
    {
        var outValue = new List<MeritCycleRequest>();
        foreach (var item in items)
        {
            var derived = MeritCycleRequest.NewMeritCycleRequest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceMeritCycleRequest(MeritCycleRequest item)
    {
        return FallbackPayBandBatchPolicy.NormalizeFallbackPayBandBatchCode(item.Code);
    }
}
