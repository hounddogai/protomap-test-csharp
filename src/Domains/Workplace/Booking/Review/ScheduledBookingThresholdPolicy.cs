namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled booking threshold policy.</summary>
public static class ScheduledBookingThresholdPolicy
{
    public static int ScheduledBookingThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double ScheduledBookingThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
