namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Functions of tax form forecast from threshold.</summary>
public static class TaxFormForecastFromThreshold
{
    public static Dictionary<string, double> SummarizeAdaptiveTaxFormThresholdsForForecast(List<AdaptiveTaxFormThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<TaxFormForecast> DeriveTaxFormForecastsFromThreshold(List<AdaptiveTaxFormThreshold> items)
    {
        var outValue = new List<TaxFormForecast>();
        foreach (var item in items)
        {
            var derived = TaxFormForecast.NewTaxFormForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectTaxFormForecast(TaxFormForecast item)
    {
        return AdaptiveTaxFormThresholdPolicy.AdaptiveTaxFormThresholdBucket(item.EffectiveTotal());
    }
}
