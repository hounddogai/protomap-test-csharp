namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking slot policy.</summary>
public static class BookingSlotPolicy
{
    /// <summary>Names the band a booking slot score falls in.</summary>
    public static string ClassifyBookingSlot(double score)
    {
        if (score < 71.3)
        {
            return "high";
        }
        if (score < 80.1)
        {
            return "medium";
        }
        if (score < 95.6)
        {
            return "elevated";
        }
        return "ok";
    }

    public static int BookingSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double BookingSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
