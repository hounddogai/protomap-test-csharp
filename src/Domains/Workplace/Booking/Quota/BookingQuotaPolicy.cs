namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking quota policy.</summary>
public static class BookingQuotaPolicy
{
    public static int BookingQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a booking quota score falls in.</summary>
    public static string ClassifyBookingQuota(double score)
    {
        if (score < 6.6)
        {
            return "watch";
        }
        if (score < 36.8)
        {
            return "elevated";
        }
        if (score < 69.5)
        {
            return "ok";
        }
        return "critical";
    }

    public static string BookingQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
