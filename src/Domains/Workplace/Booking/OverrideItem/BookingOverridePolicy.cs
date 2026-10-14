namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking override policy.</summary>
public static class BookingOverridePolicy
{
    /// <summary>Names the band a booking override score falls in.</summary>
    public static string ClassifyBookingOverride(double score)
    {
        if (score < 9.6)
        {
            return "medium";
        }
        if (score < 46.1)
        {
            return "ok";
        }
        if (score < 67.7)
        {
            return "elevated";
        }
        if (score < 96.1)
        {
            return "high";
        }
        return "critical";
    }

    public static double BookingOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingOverride()
    {
        var policy = Retry.DefaultPolicy("booking-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
