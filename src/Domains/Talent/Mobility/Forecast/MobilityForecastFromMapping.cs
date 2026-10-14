namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Functions of mobility forecast from mapping.</summary>
public static class MobilityForecastFromMapping
{
    public static Dictionary<string, double> SummarizePoolMappingsForForecast(List<PoolMapping> items)
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

    public static List<MobilityForecast> DeriveMobilityForecastsFromMapping(List<PoolMapping> items)
    {
        var outValue = new List<MobilityForecast>();
        foreach (var item in items)
        {
            var derived = MobilityForecast.NewMobilityForecast(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeMobilityForecast(MobilityForecast item)
    {
        return PoolMappingPolicy.NormalizePoolMappingCode(item.Code);
    }
}
