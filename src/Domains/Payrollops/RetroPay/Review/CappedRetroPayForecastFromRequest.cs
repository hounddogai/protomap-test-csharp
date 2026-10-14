namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Functions of capped retro pay forecast from request.</summary>
public static class CappedRetroPayForecastFromRequest
{
    public static Dictionary<string, double> SummarizePayRunRequestsForForecast(List<PayRunRequest> items)
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

    public static List<CappedRetroPayForecast> DeriveCappedRetroPayForecastsFromRequest(List<PayRunRequest> items)
    {
        var outValue = new List<CappedRetroPayForecast>();
        foreach (var item in items)
        {
            var derived = CappedRetroPayForecast.NewCappedRetroPayForecast(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeCappedRetroPayForecast(CappedRetroPayForecast item)
    {
        return PayRunRequestPolicy.ClassifyPayRunRequest(item.ProjectedVolume());
    }
}
