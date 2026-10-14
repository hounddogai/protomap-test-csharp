namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Functions of legacy pay run request from estimate.</summary>
public static class LegacyPayRunRequestFromEstimate
{
    public static Dictionary<string, double> SummarizeBatchedRetroPayEstimatesForRequest(List<BatchedRetroPayEstimate> items)
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

    public static List<LegacyPayRunRequest> DeriveLegacyPayRunRequestsFromEstimate(List<BatchedRetroPayEstimate> items)
    {
        var outValue = new List<LegacyPayRunRequest>();
        foreach (var item in items)
        {
            var derived = LegacyPayRunRequest.NewLegacyPayRunRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchLegacyPayRunRequest(LegacyPayRunRequest item)
    {
        return LenientRetroPaySummaryPolicy.NormalizeLenientRetroPaySummaryCode(item.Code);
    }
}
