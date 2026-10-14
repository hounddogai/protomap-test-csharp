namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Functions of tiered booking forecast from exception.</summary>
public static class TieredBookingForecastFromException
{
    public static Dictionary<string, double> SummarizeOccupancyExceptionsForForecast(List<OccupancyException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<TieredBookingForecast> DeriveTieredBookingForecastsFromException(List<OccupancyException> items)
    {
        var outValue = new List<TieredBookingForecast>();
        foreach (var item in items)
        {
            var derived = TieredBookingForecast.NewTieredBookingForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectTieredBookingForecast(TieredBookingForecast item)
    {
        return OccupancyAllocationPolicy.OccupancyAllocationWeight(5, true);
    }
}
