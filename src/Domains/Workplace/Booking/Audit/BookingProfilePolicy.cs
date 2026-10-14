namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking profile policy.</summary>
public static class BookingProfilePolicy
{
    /// <summary>Puts a booking profile code in its canonical BP- form.</summary>
    public static string NormalizeBookingProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BookingProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
