namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Functions of retro pay claim from forecast.</summary>
public static class RetroPayClaimFromForecast
{
    public static Dictionary<string, double> SummarizeTieredRetroPayForecastsForClaim(List<TieredRetroPayForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<RetroPayClaim> DeriveRetroPayClaimsFromForecast(List<TieredRetroPayForecast> items)
    {
        var outValue = new List<RetroPayClaim>();
        foreach (var item in items)
        {
            var derived = RetroPayClaim.NewRetroPayClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRetroPayClaim(RetroPayClaim item)
    {
        return RetroPayForecastPolicy.RetroPayForecastBucket(item.ProjectedExposure());
    }
}
