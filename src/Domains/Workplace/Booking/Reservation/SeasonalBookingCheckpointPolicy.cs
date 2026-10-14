namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal booking checkpoint policy.</summary>
public static class SeasonalBookingCheckpointPolicy
{
    public static double SeasonalBookingCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 70841.0, in order.</summary>
    public static int SeasonalBookingCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70841.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
