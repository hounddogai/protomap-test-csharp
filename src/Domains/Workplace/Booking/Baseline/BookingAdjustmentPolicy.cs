namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking adjustment policy.</summary>
public static class BookingAdjustmentPolicy
{
    /// <summary>Names the band a booking adjustment score falls in.</summary>
    public static string ClassifyBookingAdjustment(double score)
    {
        if (score < 10.5)
        {
            return "medium";
        }
        if (score < 15.8)
        {
            return "minimal";
        }
        if (score < 78.0)
        {
            return "watch";
        }
        if (score < 82.4)
        {
            return "ok";
        }
        return "critical";
    }

    public static int BookingAdjustmentSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingAdjustment()
    {
        var policy = Retry.DefaultPolicy("booking-adjustment");
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

    public static string BookingAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
