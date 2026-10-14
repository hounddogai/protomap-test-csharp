namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling career path cycle policy.</summary>
public static class RollingCareerPathCyclePolicy
{
    /// <summary>Puts a rolling career path cycle code in its canonical RCPC- form.</summary>
    public static string NormalizeRollingCareerPathCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RCPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RCPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRollingCareerPathCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
