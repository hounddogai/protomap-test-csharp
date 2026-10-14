namespace Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Functions of rolling booking review policy.</summary>
public static class RollingBookingReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67016.0, in order.</summary>
    public static int RollingBookingReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67016.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling booking review score falls in.</summary>
    public static string ClassifyRollingBookingReview(double score)
    {
        if (score < 22.0)
        {
            return "minimal";
        }
        if (score < 31.8)
        {
            return "medium";
        }
        return "low";
    }
}
