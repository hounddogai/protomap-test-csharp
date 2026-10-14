namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Functions of weighted access badge exception from forecast.</summary>
public static class WeightedAccessBadgeExceptionFromForecast
{
    public static Dictionary<string, double> SummarizeOccupancyForecastsForException(List<OccupancyForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<WeightedAccessBadgeException> DeriveWeightedAccessBadgeExceptionsFromForecast(List<OccupancyForecast> items)
    {
        var outValue = new List<WeightedAccessBadgeException>();
        foreach (var item in items)
        {
            var derived = WeightedAccessBadgeException.NewWeightedAccessBadgeException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveWeightedAccessBadgeExceptionExtra(WeightedAccessBadgeException item)
    {
        return OccupancyForecastPolicy.OccupancyForecastWeight(7, true);
    }
}
