namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Functions of accrual quota from variance.</summary>
public static class AccrualQuotaFromVariance
{
    public static Dictionary<string, double> SummarizeAdaptiveHolidayVariancesForQuota(List<AdaptiveHolidayVariance> items)
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

    public static List<AccrualQuota> DeriveAccrualQuotasFromVariance(List<AdaptiveHolidayVariance> items)
    {
        var outValue = new List<AccrualQuota>();
        foreach (var item in items)
        {
            var derived = AccrualQuota.NewAccrualQuota(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchAccrualQuota(AccrualQuota item)
    {
        return LegacyHolidayBundlePolicy.LegacyHolidayBundleWeight(1, true);
    }
}
