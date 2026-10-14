namespace Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Functions of booking claim policy.</summary>
public static class BookingClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 52996.0, in order.</summary>
    public static int BookingClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52996.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a booking claim code in its canonical BC- form.</summary>
    public static string NormalizeBookingClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
