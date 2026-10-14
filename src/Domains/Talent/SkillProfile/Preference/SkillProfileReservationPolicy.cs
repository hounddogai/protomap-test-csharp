namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile reservation policy.</summary>
public static class SkillProfileReservationPolicy
{
    /// <summary>Names the band a skill profile reservation score falls in.</summary>
    public static string ClassifySkillProfileReservation(double score)
    {
        if (score < 1.4)
        {
            return "ok";
        }
        if (score < 17.2)
        {
            return "high";
        }
        if (score < 85.2)
        {
            return "low";
        }
        return "medium";
    }

    public static double SkillProfileReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a skill profile reservation code in its canonical SPR- form.</summary>
    public static string NormalizeSkillProfileReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
