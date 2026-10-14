namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking score policy.</summary>
public static class BookingScorePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingScore()
    {
        var policy = Retry.DefaultPolicy("booking-score");
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

    public static int BookingScoreSpan(int start, int end)
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

    /// <summary>Puts a booking score code in its canonical BS- form.</summary>
    public static string NormalizeBookingScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a booking score score falls in.</summary>
    public static string ClassifyBookingScore(double score)
    {
        if (score < 3.8)
        {
            return "high";
        }
        if (score < 84.9)
        {
            return "watch";
        }
        if (score < 86.6)
        {
            return "low";
        }
        return "elevated";
    }
}
