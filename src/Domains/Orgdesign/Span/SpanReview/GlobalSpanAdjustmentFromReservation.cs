namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Functions of global span adjustment from reservation.</summary>
public static class GlobalSpanAdjustmentFromReservation
{
    public static Dictionary<string, double> SummarizeBatchedSpanReservationsForAdjustment(List<BatchedSpanReservation> items)
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

    public static List<GlobalSpanAdjustment> DeriveGlobalSpanAdjustmentsFromReservation(List<BatchedSpanReservation> items)
    {
        var outValue = new List<GlobalSpanAdjustment>();
        foreach (var item in items)
        {
            var derived = GlobalSpanAdjustment.NewGlobalSpanAdjustment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveGlobalSpanAdjustment(GlobalSpanAdjustment item)
    {
        return BatchedSpanReservationPolicy.BatchedSpanReservationBucket(item.AdjustedExposure());
    }
}
