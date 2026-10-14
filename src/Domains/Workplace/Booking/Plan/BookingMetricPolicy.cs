namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking metric policy.</summary>
public static class BookingMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 88163.0, in order.</summary>
    public static int BookingMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88163.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string BookingMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
