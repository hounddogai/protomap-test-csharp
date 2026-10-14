namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking notice policy.</summary>
public static class BookingNoticePolicy
{
    public static string BookingNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a booking notice score falls in.</summary>
    public static string ClassifyBookingNotice(double score)
    {
        if (score < 7.8)
        {
            return "watch";
        }
        if (score < 49.0)
        {
            return "ok";
        }
        if (score < 55.0)
        {
            return "critical";
        }
        return "minimal";
    }

    public static double BookingNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
