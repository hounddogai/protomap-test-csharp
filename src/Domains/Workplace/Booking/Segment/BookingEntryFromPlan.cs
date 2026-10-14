namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Functions of booking entry from plan.</summary>
public static class BookingEntryFromPlan
{
    public static Dictionary<string, double> SummarizeScheduledOccupancyPlansForEntry(List<ScheduledOccupancyPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<BookingEntry> DeriveBookingEntriesFromPlan(List<ScheduledOccupancyPlan> items)
    {
        var outValue = new List<BookingEntry>();
        foreach (var item in items)
        {
            var derived = BookingEntry.NewBookingEntry(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreBookingEntry(BookingEntry item)
    {
        return ScheduledOccupancyPlanPolicy.NormalizeScheduledOccupancyPlanCode(item.Code);
    }
}
