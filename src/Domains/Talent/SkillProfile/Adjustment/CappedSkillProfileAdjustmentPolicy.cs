namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Functions of capped skill profile adjustment policy.</summary>
public static class CappedSkillProfileAdjustmentPolicy
{
    /// <summary>Puts a capped skill profile adjustment code in its canonical CSPA- form.</summary>
    public static string NormalizeCappedSkillProfileAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CSPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CSPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped skill profile adjustment score falls in.</summary>
    public static string ClassifyCappedSkillProfileAdjustment(double score)
    {
        if (score < 16.3)
        {
            return "watch";
        }
        if (score < 39.9)
        {
            return "minimal";
        }
        if (score < 48.3)
        {
            return "elevated";
        }
        if (score < 57.2)
        {
            return "critical";
        }
        return "medium";
    }
}
