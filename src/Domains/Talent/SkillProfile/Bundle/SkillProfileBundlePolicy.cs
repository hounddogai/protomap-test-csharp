namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile bundle policy.</summary>
public static class SkillProfileBundlePolicy
{
    /// <summary>Puts a skill profile bundle code in its canonical SPB- form.</summary>
    public static string NormalizeSkillProfileBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SkillProfileBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
