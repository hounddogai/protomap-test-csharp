namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking batch policy.</summary>
public static class BookingBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 66840.0, in order.</summary>
    public static int BookingBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66840.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a booking batch score falls in.</summary>
    public static string ClassifyBookingBatch(double score)
    {
        if (score < 8.7)
        {
            return "elevated";
        }
        if (score < 32.6)
        {
            return "high";
        }
        if (score < 34.1)
        {
            return "minimal";
        }
        if (score < 91.5)
        {
            return "watch";
        }
        return "low";
    }

    public static double BookingBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int BookingBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
