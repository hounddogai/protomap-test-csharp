namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Functions of seasonal pay band quota from request.</summary>
public static class SeasonalPayBandQuotaFromRequest
{
    public static Dictionary<string, double> SummarizeEquityGrantRequestsForQuota(List<EquityGrantRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<SeasonalPayBandQuota> DeriveSeasonalPayBandQuotasFromRequest(List<EquityGrantRequest> items)
    {
        var outValue = new List<SeasonalPayBandQuota>();
        foreach (var item in items)
        {
            var derived = SeasonalPayBandQuota.NewSeasonalPayBandQuota(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveSeasonalPayBandQuota(SeasonalPayBandQuota item)
    {
        return EquityGrantRequestPolicy.EquityGrantRequestWeight(2, true);
    }
}
