namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday tier policy.</summary>
public static class HolidayTierPolicy
{
    /// <summary>Puts a holiday tier code in its canonical HT- form.</summary>
    public static string NormalizeHolidayTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string HolidayTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 42207.0, in order.</summary>
    public static int HolidayTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42207.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a holiday tier score falls in.</summary>
    public static string ClassifyHolidayTier(double score)
    {
        if (score < 34.2)
        {
            return "low";
        }
        if (score < 73.9)
        {
            return "elevated";
        }
        return "medium";
    }
}
