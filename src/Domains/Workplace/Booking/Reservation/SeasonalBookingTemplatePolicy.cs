namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal booking template policy.</summary>
public static class SeasonalBookingTemplatePolicy
{
    public static int SeasonalBookingTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a seasonal booking template code in its canonical SBT- form.</summary>
    public static string NormalizeSeasonalBookingTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalBookingTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 71193.0, in order.</summary>
    public static int SeasonalBookingTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71193.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
