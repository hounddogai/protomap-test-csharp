namespace Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Functions of booking request policy.</summary>
public static class BookingRequestPolicy
{
    public static int BookingRequestSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a booking request code in its canonical BR- form.</summary>
    public static string NormalizeBookingRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
