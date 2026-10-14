namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Functions of weighted booking variance from checkpoint.</summary>
public static class WeightedBookingVarianceFromCheckpoint
{
    public static Dictionary<string, double> SummarizeLegacyVisitorCheckpointsForVariance(List<LegacyVisitorCheckpoint> items)
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

    public static List<WeightedBookingVariance> DeriveWeightedBookingVariancesFromCheckpoint(List<LegacyVisitorCheckpoint> items)
    {
        var outValue = new List<WeightedBookingVariance>();
        foreach (var item in items)
        {
            var derived = WeightedBookingVariance.NewWeightedBookingVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeWeightedBookingVariance(WeightedBookingVariance item)
    {
        return LenientVisitorPreferencePolicy.LenientVisitorPreferenceBucket(item.NetLoad());
    }
}
