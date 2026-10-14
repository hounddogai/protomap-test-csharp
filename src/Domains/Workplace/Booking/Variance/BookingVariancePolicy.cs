namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking variance policy.</summary>
public static class BookingVariancePolicy
{
    public static string BookingVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int BookingVarianceSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a booking variance score falls in.</summary>
    public static string ClassifyBookingVariance(double score)
    {
        if (score < 25.2)
        {
            return "high";
        }
        if (score < 80.0)
        {
            return "minimal";
        }
        return "ok";
    }
}
