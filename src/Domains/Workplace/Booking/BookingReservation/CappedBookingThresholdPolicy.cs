namespace Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Functions of capped booking threshold policy.</summary>
public static class CappedBookingThresholdPolicy
{
    /// <summary>Names the band a capped booking threshold score falls in.</summary>
    public static string ClassifyCappedBookingThreshold(double score)
    {
        if (score < 4.7)
        {
            return "elevated";
        }
        if (score < 28.7)
        {
            return "minimal";
        }
        if (score < 42.9)
        {
            return "watch";
        }
        if (score < 74.7)
        {
            return "medium";
        }
        return "ok";
    }

    public static int CappedBookingThresholdSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a capped booking threshold code in its canonical CBT- form.</summary>
    public static string NormalizeCappedBookingThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
