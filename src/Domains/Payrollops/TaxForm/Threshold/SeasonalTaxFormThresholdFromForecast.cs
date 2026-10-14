namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Functions of seasonal tax form threshold from forecast.</summary>
public static class SeasonalTaxFormThresholdFromForecast
{
    public static Dictionary<string, double> SummarizeTaxFormForecastsForThreshold(List<TaxFormForecast> items)
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

    public static List<SeasonalTaxFormThreshold> DeriveSeasonalTaxFormThresholdsFromForecast(List<TaxFormForecast> items)
    {
        var outValue = new List<SeasonalTaxFormThreshold>();
        foreach (var item in items)
        {
            var derived = SeasonalTaxFormThreshold.NewSeasonalTaxFormThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifySeasonalTaxFormThreshold(SeasonalTaxFormThreshold item)
    {
        return TaxFormForecastPolicy.ClassifyTaxFormForecast(item.ProjectedExposure());
    }
}
