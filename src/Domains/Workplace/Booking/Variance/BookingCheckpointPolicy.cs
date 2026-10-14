namespace Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Functions of booking checkpoint policy.</summary>
public static class BookingCheckpointPolicy
{
    public static int BookingCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a booking checkpoint score falls in.</summary>
    public static string ClassifyBookingCheckpoint(double score)
    {
        if (score < 14.3)
        {
            return "critical";
        }
        if (score < 29.7)
        {
            return "elevated";
        }
        if (score < 52.8)
        {
            return "medium";
        }
        if (score < 73.3)
        {
            return "ok";
        }
        return "low";
    }
}
