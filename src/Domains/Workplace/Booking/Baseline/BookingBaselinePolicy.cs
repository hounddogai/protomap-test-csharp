namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking baseline policy.</summary>
public static class BookingBaselinePolicy
{
    public static string BookingBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int BookingBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a booking baseline score falls in.</summary>
    public static string ClassifyBookingBaseline(double score)
    {
        if (score < 18.5)
        {
            return "medium";
        }
        if (score < 19.4)
        {
            return "elevated";
        }
        if (score < 43.2)
        {
            return "critical";
        }
        if (score < 55.5)
        {
            return "ok";
        }
        return "minimal";
    }
}
