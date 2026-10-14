namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Functions of booking threshold from proposal.</summary>
public static class BookingThresholdFromProposal
{
    public static Dictionary<string, double> SummarizeOccupancyProposalsForThreshold(List<OccupancyProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<BookingThreshold> DeriveBookingThresholdsFromProposal(List<OccupancyProposal> items)
    {
        var outValue = new List<BookingThreshold>();
        foreach (var item in items)
        {
            var derived = BookingThreshold.NewBookingThreshold(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyBookingThreshold(BookingThreshold item)
    {
        return OccupancyRecordPolicy.NormalizeOccupancyRecordCode(item.Code);
    }
}
