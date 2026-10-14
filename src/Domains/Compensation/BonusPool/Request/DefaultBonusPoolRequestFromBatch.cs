namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Functions of default bonus pool request from batch.</summary>
public static class DefaultBonusPoolRequestFromBatch
{
    public static Dictionary<string, double> SummarizeEquityGrantBatchesForRequest(List<EquityGrantBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<DefaultBonusPoolRequest> DeriveDefaultBonusPoolRequestsFromBatch(List<EquityGrantBatch> items)
    {
        var outValue = new List<DefaultBonusPoolRequest>();
        foreach (var item in items)
        {
            var derived = DefaultBonusPoolRequest.NewDefaultBonusPoolRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveDefaultBonusPoolRequest(DefaultBonusPoolRequest item)
    {
        return EquityGrantReviewPolicy.EquityGrantReviewWeight(7, true);
    }
}
