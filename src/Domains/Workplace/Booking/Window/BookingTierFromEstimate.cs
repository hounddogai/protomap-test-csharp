namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Functions of booking tier from estimate.</summary>
public static class BookingTierFromEstimate
{
    public static Dictionary<string, double> SummarizeVisitorEstimatesForTier(List<VisitorEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BookingTier> DeriveBookingTiersFromEstimate(List<VisitorEstimate> items)
    {
        var outValue = new List<BookingTier>();
        foreach (var item in items)
        {
            var derived = BookingTier.NewBookingTier(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBookingTier(BookingTier item)
    {
        return GlobalVisitorDraftPolicy.GlobalVisitorDraftBucket(item.AdjustedValue());
    }
}
