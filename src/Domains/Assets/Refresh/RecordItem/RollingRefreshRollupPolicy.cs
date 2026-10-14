namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling refresh rollup policy.</summary>
public static class RollingRefreshRollupPolicy
{
    /// <summary>Names the band a rolling refresh rollup score falls in.</summary>
    public static string ClassifyRollingRefreshRollup(double score)
    {
        if (score < 3.8)
        {
            return "low";
        }
        if (score < 40.8)
        {
            return "elevated";
        }
        if (score < 60.7)
        {
            return "critical";
        }
        if (score < 86.6)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Puts a rolling refresh rollup code in its canonical RRR- form.</summary>
    public static string NormalizeRollingRefreshRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRollingRefreshRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
