namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional booking reservation policy.</summary>
public static class RegionalBookingReservationPolicy
{
    public static string RegionalBookingReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a regional booking reservation score falls in.</summary>
    public static string ClassifyRegionalBookingReservation(double score)
    {
        if (score < 16.6)
        {
            return "elevated";
        }
        if (score < 68.5)
        {
            return "minimal";
        }
        return "high";
    }
}
