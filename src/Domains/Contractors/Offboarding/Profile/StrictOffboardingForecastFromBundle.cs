namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>Functions of strict offboarding forecast from bundle.</summary>
public static class StrictOffboardingForecastFromBundle
{
    public static Dictionary<string, double> SummarizeVendorBundlesForForecast(List<VendorBundle> items)
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

    public static List<StrictOffboardingForecast> DeriveStrictOffboardingForecastsFromBundle(List<VendorBundle> items)
    {
        var outValue = new List<StrictOffboardingForecast>();
        foreach (var item in items)
        {
            var derived = StrictOffboardingForecast.NewStrictOffboardingForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceStrictOffboardingForecast(StrictOffboardingForecast item)
    {
        return VendorMetricPolicy.VendorMetricWeight(1, true);
    }
}
