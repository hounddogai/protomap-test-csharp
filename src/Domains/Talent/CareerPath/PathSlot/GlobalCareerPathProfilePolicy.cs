namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global career path profile policy.</summary>
public static class GlobalCareerPathProfilePolicy
{
    public static List<string> MergeGlobalCareerPathProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a global career path profile code in its canonical GCPP- form.</summary>
    public static string NormalizeGlobalCareerPathProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GCPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GCPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GlobalCareerPathProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
