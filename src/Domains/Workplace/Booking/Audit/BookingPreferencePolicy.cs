namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking preference policy.</summary>
public static class BookingPreferencePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36331.0, in order.</summary>
    public static int BookingPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36331.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double BookingPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
