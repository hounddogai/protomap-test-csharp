namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Functions of pay band request from quota.</summary>
public static class PayBandRequestFromQuota
{
    public static Dictionary<string, double> SummarizePayBandQuotasForRequest(List<PayBandQuota> items)
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

    public static List<PayBandRequest> DerivePayBandRequestsFromQuota(List<PayBandQuota> items)
    {
        var outValue = new List<PayBandRequest>();
        foreach (var item in items)
        {
            var derived = PayBandRequest.NewPayBandRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocatePayBandRequest(PayBandRequest item)
    {
        return PayBandForecastPolicy.PayBandForecastWeight(3, true);
    }
}
