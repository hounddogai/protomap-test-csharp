namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling pool entry policy.</summary>
public static class RollingPoolEntryPolicy
{
    public static List<string> MergeRollingPoolEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rolling pool entry score falls in.</summary>
    public static string ClassifyRollingPoolEntry(double score)
    {
        if (score < 38.3)
        {
            return "low";
        }
        if (score < 93.5)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Puts a rolling pool entry code in its canonical RPE- form.</summary>
    public static string NormalizeRollingPoolEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
