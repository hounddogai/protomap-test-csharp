namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking plan policy.</summary>
public static class BookingPlanPolicy
{
    public static string BookingPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingPlan()
    {
        var policy = Retry.DefaultPolicy("booking-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int BookingPlanSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a booking plan score falls in.</summary>
    public static string ClassifyBookingPlan(double score)
    {
        if (score < 39.5)
        {
            return "critical";
        }
        if (score < 45.8)
        {
            return "elevated";
        }
        return "watch";
    }
}
