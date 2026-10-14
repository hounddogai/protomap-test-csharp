namespace Acme.Workforce.Workplace.Booking.Review;

/// <summary>Functions of booking digest policy.</summary>
public static class BookingDigestPolicy
{
    /// <summary>Puts a booking digest code in its canonical BD- form.</summary>
    public static string NormalizeBookingDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BookingDigestSpan(int start, int end)
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
