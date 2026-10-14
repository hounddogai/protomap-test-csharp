namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled booking claim policy.</summary>
public static class ScheduledBookingClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 117.0, in order.</summary>
    public static int ScheduledBookingClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 117.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled booking claim code in its canonical SBC- form.</summary>
    public static string NormalizeScheduledBookingClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledBookingClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
