namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Functions of booking score from milestone.</summary>
public static class BookingScoreFromMilestone
{
    public static Dictionary<string, double> SummarizeDeskMilestonesForScore(List<DeskMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<BookingScore> DeriveBookingScoresFromMilestone(List<DeskMilestone> items)
    {
        var outValue = new List<BookingScore>();
        foreach (var item in items)
        {
            var derived = BookingScore.NewBookingScore(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBookingScore(BookingScore item)
    {
        return DeskCheckpointPolicy.DeskCheckpointBucket(item.ProjectedTotal());
    }
}
