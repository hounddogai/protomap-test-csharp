namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal skill profile preference policy.</summary>
public static class SeasonalSkillProfilePreferencePolicy
{
    /// <summary>Puts a seasonal skill profile preference code in its canonical SSPP- form.</summary>
    public static string NormalizeSeasonalSkillProfilePreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal skill profile preference score falls in.</summary>
    public static string ClassifySeasonalSkillProfilePreference(double score)
    {
        if (score < 4.4)
        {
            return "critical";
        }
        if (score < 60.6)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 89828.0, in order.</summary>
    public static int SeasonalSkillProfilePreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89828.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SeasonalSkillProfilePreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
