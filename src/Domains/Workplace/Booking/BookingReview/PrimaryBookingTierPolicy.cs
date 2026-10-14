namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary booking tier policy.</summary>
public static class PrimaryBookingTierPolicy
{
    public static string PrimaryBookingTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a primary booking tier code in its canonical PBT- form.</summary>
    public static string NormalizePrimaryBookingTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 36706.0, in order.</summary>
    public static int PrimaryBookingTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36706.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
