namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Functions of strict accrual forecast from policy.</summary>
public static class StrictAccrualForecastFromPolicy
{
    public static Dictionary<string, double> SummarizeRegionalBalancePoliciesForForecast(List<RegionalBalancePolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<StrictAccrualForecast> DeriveStrictAccrualForecastsFromPolicy(List<RegionalBalancePolicy> items)
    {
        var outValue = new List<StrictAccrualForecast>();
        foreach (var item in items)
        {
            var derived = StrictAccrualForecast.NewStrictAccrualForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyStrictAccrualForecast(StrictAccrualForecast item)
    {
        return ManualBalanceMetricPolicy.ManualBalanceMetricBucket(item.MeasuredValue());
    }
}
