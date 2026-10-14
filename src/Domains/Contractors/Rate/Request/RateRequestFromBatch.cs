namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Functions of rate request from batch.</summary>
public static class RateRequestFromBatch
{
    public static Dictionary<string, double> SummarizeRateBatchesForRequest(List<RateBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<RateRequest> DeriveRateRequestsFromBatch(List<RateBatch> items)
    {
        var outValue = new List<RateRequest>();
        foreach (var item in items)
        {
            var derived = RateRequest.NewRateRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileRateRequest(RateRequest item)
    {
        return RateNoticePolicy.ClassifyRateNotice(item.MeasuredValue());
    }
}
