namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Functions of warranty forecast from target.</summary>
public static class WarrantyForecastFromTarget
{
    public static Dictionary<string, double> SummarizeSeasonalWarrantyTargetsForForecast(List<SeasonalWarrantyTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<WarrantyForecast> DeriveWarrantyForecastsFromTarget(List<SeasonalWarrantyTarget> items)
    {
        var outValue = new List<WarrantyForecast>();
        foreach (var item in items)
        {
            var derived = WarrantyForecast.NewWarrantyForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreWarrantyForecast(WarrantyForecast item)
    {
        return WarrantyMilestonePolicy.NormalizeWarrantyMilestoneCode(item.Code);
    }
}
