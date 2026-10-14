namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive skill profile summary policy.</summary>
public static class AdaptiveSkillProfileSummaryPolicy
{
    /// <summary>Puts a adaptive skill profile summary code in its canonical ASPS- form.</summary>
    public static string NormalizeAdaptiveSkillProfileSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ASPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ASPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AdaptiveSkillProfileSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergeAdaptiveSkillProfileSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
