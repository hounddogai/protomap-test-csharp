namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted booking entry policy.</summary>
public static class WeightedBookingEntryPolicy
{
    public static string WeightedBookingEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 2870.0, in order.</summary>
    public static int WeightedBookingEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2870.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
