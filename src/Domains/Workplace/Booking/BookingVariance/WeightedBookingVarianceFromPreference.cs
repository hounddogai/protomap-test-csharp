namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Workplace.Occupancy.Mapping;

/// <summary>Functions of weighted booking variance from preference.</summary>
public static class WeightedBookingVarianceFromPreference
{
    public static Dictionary<string, double> SummarizeFallbackOccupancyPreferencesForVariance(List<FallbackOccupancyPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<WeightedBookingVariance> DeriveWeightedBookingVariancesFromPreference(List<FallbackOccupancyPreference> items)
    {
        var outValue = new List<WeightedBookingVariance>();
        foreach (var item in items)
        {
            var derived = WeightedBookingVariance.NewWeightedBookingVariance(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }
}
