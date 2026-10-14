namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking bundle policy.</summary>
public static class BookingBundlePolicy
{
    /// <summary>Puts a booking bundle code in its canonical BB- form.</summary>
    public static string NormalizeBookingBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingBundle()
    {
        var policy = Retry.DefaultPolicy("booking-bundle");
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

    /// <summary>Names the band a booking bundle score falls in.</summary>
    public static string ClassifyBookingBundle(double score)
    {
        if (score < 5.0)
        {
            return "high";
        }
        if (score < 17.5)
        {
            return "medium";
        }
        if (score < 50.4)
        {
            return "minimal";
        }
        if (score < 54.4)
        {
            return "critical";
        }
        return "elevated";
    }

    public static int BookingBundleSpan(int start, int end)
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
