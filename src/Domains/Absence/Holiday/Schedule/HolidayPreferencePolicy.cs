namespace Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Functions of holiday preference policy.</summary>
public static class HolidayPreferencePolicy
{
    /// <summary>Puts a holiday preference code in its canonical HP- form.</summary>
    public static string NormalizeHolidayPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int HolidayPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a holiday preference score falls in.</summary>
    public static string ClassifyHolidayPreference(double score)
    {
        if (score < 53.0)
        {
            return "minimal";
        }
        if (score < 71.0)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 57533.0, in order.</summary>
    public static int HolidayPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57533.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
