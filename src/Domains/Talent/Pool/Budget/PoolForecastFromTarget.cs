namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Functions of pool forecast from target.</summary>
public static class PoolForecastFromTarget
{
    public static Dictionary<string, double> SummarizePoolTargetsForForecast(List<PoolTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PoolForecast> DerivePoolForecastsFromTarget(List<PoolTarget> items)
    {
        var outValue = new List<PoolForecast>();
        foreach (var item in items)
        {
            var derived = PoolForecast.NewPoolForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPoolForecast(PoolForecast item)
    {
        return PoolMetricPolicy.NormalizePoolMetricCode(item.Code);
    }
}
