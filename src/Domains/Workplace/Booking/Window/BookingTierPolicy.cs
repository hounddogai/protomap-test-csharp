namespace Acme.Workforce.Workplace.Booking.Window;

/// <summary>Functions of booking tier policy.</summary>
public static class BookingTierPolicy
{
    /// <summary>Puts a booking tier code in its canonical BT- form.</summary>
    public static string NormalizeBookingTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 22231.0, in order.</summary>
    public static int BookingTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22231.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
