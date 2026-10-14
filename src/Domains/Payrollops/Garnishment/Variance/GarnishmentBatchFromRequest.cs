namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Functions of garnishment batch from request.</summary>
public static class GarnishmentBatchFromRequest
{
    public static Dictionary<string, double> SummarizeLegacyPayRunRequestsForBatch(List<LegacyPayRunRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<GarnishmentBatch> DeriveGarnishmentBatchesFromRequest(List<LegacyPayRunRequest> items)
    {
        var outValue = new List<GarnishmentBatch>();
        foreach (var item in items)
        {
            var derived = GarnishmentBatch.NewGarnishmentBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeGarnishmentBatch(GarnishmentBatch item)
    {
        return PayRunVariancePolicy.NormalizePayRunVarianceCode(item.Code);
    }
}
