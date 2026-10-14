namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile agreement policy.</summary>
public static class SkillProfileAgreementPolicy
{
    /// <summary>Names the band a skill profile agreement score falls in.</summary>
    public static string ClassifySkillProfileAgreement(double score)
    {
        if (score < 61.7)
        {
            return "minimal";
        }
        if (score < 82.0)
        {
            return "medium";
        }
        if (score < 97.7)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Puts a skill profile agreement code in its canonical SPA- form.</summary>
    public static string NormalizeSkillProfileAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SkillProfileAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int SkillProfileAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
