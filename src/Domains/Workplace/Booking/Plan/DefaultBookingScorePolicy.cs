namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default booking score policy.</summary>
public static class DefaultBookingScorePolicy
{
    public static int DefaultBookingScoreSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultBookingScore()
    {
        var policy = Retry.DefaultPolicy("default-booking-score");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
