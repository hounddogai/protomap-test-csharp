namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh rollup policy.</summary>
public static class RefreshRollupPolicy
{
    public static List<string> MergeRefreshRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a refresh rollup code in its canonical RR- form.</summary>
    public static string NormalizeRefreshRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a refresh rollup score falls in.</summary>
    public static string ClassifyRefreshRollup(double score)
    {
        if (score < 33.7)
        {
            return "elevated";
        }
        if (score < 65.0)
        {
            return "watch";
        }
        if (score < 93.9)
        {
            return "minimal";
        }
        return "low";
    }
}
