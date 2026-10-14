namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Functions of booking forecast from proposal.</summary>
public static class BookingForecastFromProposal
{
    public static Dictionary<string, double> SummarizeVisitorProposalsForForecast(List<VisitorProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<BookingForecast> DeriveBookingForecastsFromProposal(List<VisitorProposal> items)
    {
        var outValue = new List<BookingForecast>();
        foreach (var item in items)
        {
            var derived = BookingForecast.NewBookingForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeBookingForecast(BookingForecast item)
    {
        return VisitorExceptionPolicy.NormalizeVisitorExceptionCode(item.Code);
    }
}
