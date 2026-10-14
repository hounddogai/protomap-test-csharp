namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional booking preference policy.</summary>
public static class RegionalBookingPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalBookingPreference()
    {
        var policy = Retry.DefaultPolicy("regional-booking-preference");
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

    public static int RegionalBookingPreferenceSpan(int start, int end)
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
}
