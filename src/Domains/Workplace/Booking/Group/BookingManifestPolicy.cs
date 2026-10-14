namespace Acme.Workforce.Workplace.Booking.Group;

/// <summary>Functions of booking manifest policy.</summary>
public static class BookingManifestPolicy
{
    /// <summary>Puts a booking manifest code in its canonical BM- form.</summary>
    public static string NormalizeBookingManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a booking manifest score falls in.</summary>
    public static string ClassifyBookingManifest(double score)
    {
        if (score < 18.1)
        {
            return "watch";
        }
        if (score < 31.9)
        {
            return "ok";
        }
        if (score < 33.9)
        {
            return "minimal";
        }
        return "critical";
    }

    public static int BookingManifestSpan(int start, int end)
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
        return span / 3600;
    }
}
