namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Functions of adaptive booking exception from checkpoint.</summary>
public static class AdaptiveBookingExceptionFromCheckpoint
{
    public static Dictionary<string, double> SummarizeVisitorCheckpointsForException(List<VisitorCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AdaptiveBookingException> DeriveAdaptiveBookingExceptionsFromCheckpoint(List<VisitorCheckpoint> items)
    {
        var outValue = new List<AdaptiveBookingException>();
        foreach (var item in items)
        {
            var derived = AdaptiveBookingException.NewAdaptiveBookingException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAdaptiveBookingException(AdaptiveBookingException item)
    {
        return VisitorAllocationPolicy.VisitorAllocationBucket(item.WeightedValue());
    }
}
