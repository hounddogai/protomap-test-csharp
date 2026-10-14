namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Functions of legacy offboarding forecast from rollup.</summary>
public static class LegacyOffboardingForecastFromRollup
{
    public static Dictionary<string, double> SummarizeVendorRollupsForForecast(List<VendorRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LegacyOffboardingForecast> DeriveLegacyOffboardingForecastsFromRollup(List<VendorRollup> items)
    {
        var outValue = new List<LegacyOffboardingForecast>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingForecast.NewLegacyOffboardingForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLegacyOffboardingForecastExtra(LegacyOffboardingForecast item)
    {
        return VendorRollupPolicy.ClassifyVendorRollup(item.WeightedLoad());
    }
}
