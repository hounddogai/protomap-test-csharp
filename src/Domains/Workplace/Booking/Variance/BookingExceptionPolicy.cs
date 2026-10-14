namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking exception policy.</summary>
public static class BookingExceptionPolicy
{
    /// <summary>Names the band a booking exception score falls in.</summary>
    public static string ClassifyBookingException(double score)
    {
        if (score < 71.4)
        {
            return "high";
        }
        if (score < 78.6)
        {
            return "elevated";
        }
        return "ok";
    }

    /// <summary>Puts a booking exception code in its canonical BE- form.</summary>
    public static string NormalizeBookingExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingException()
    {
        var policy = Retry.DefaultPolicy("booking-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int BookingExceptionSpan(int start, int end)
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
        return span / 3600;
    }
}
