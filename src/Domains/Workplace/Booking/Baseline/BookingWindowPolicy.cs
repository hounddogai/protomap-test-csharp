namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking window policy.</summary>
public static class BookingWindowPolicy
{
    public static string BookingWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a booking window code in its canonical BW- form.</summary>
    public static string NormalizeBookingWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BookingWindowSpan(int start, int end)
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
        return span / 86400;
    }
}
