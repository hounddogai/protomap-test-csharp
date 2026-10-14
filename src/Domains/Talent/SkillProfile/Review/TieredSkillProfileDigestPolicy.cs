namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered skill profile digest policy.</summary>
public static class TieredSkillProfileDigestPolicy
{
    /// <summary>Puts a tiered skill profile digest code in its canonical TSPD- form.</summary>
    public static string NormalizeTieredSkillProfileDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TSPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TSPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredSkillProfileDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
