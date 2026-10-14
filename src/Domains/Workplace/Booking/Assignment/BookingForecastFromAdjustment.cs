namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Functions of booking forecast from adjustment.</summary>
public static class BookingForecastFromAdjustment
{
    public static Dictionary<string, double> SummarizeBookingAdjustmentsForForecast(List<BookingAdjustment> items)
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

    public static List<BookingForecast> DeriveBookingForecastsFromAdjustment(List<BookingAdjustment> items)
    {
        var outValue = new List<BookingForecast>();
        foreach (var item in items)
        {
            var derived = BookingForecast.NewBookingForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBookingForecast(BookingForecast item)
    {
        return BookingWindowPolicy.BookingWindowBucket(item.WeightedTotal());
    }
}
