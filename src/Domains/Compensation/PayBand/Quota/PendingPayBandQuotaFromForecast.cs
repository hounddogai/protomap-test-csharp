namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>Functions of pending pay band quota from forecast.</summary>
public static class PendingPayBandQuotaFromForecast
{
    public static Dictionary<string, double> SummarizeAdjustmentForecastsForQuota(List<AdjustmentForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PendingPayBandQuota> DerivePendingPayBandQuotasFromForecast(List<AdjustmentForecast> items)
    {
        var outValue = new List<PendingPayBandQuota>();
        foreach (var item in items)
        {
            var derived = PendingPayBandQuota.NewPendingPayBandQuota(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPendingPayBandQuota(PendingPayBandQuota item)
    {
        return AdjustmentForecastPolicy.NormalizeAdjustmentForecastCode(item.Code);
    }
}
