namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of skill profile policy policy.</summary>
public static class SkillProfilePolicyPolicy
{
    public static double SkillProfilePolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeSkillProfilePolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string SkillProfilePolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a skill profile policy code in its canonical SPP- form.</summary>
    public static string NormalizeSkillProfilePolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
