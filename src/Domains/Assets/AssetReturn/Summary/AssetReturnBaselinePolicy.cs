namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return baseline policy.</summary>
public static class AssetReturnBaselinePolicy
{
    public static int AssetReturnBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeAssetReturnBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a asset return baseline score falls in.</summary>
    public static string ClassifyAssetReturnBaseline(double score)
    {
        if (score < 62.0)
        {
            return "medium";
        }
        if (score < 83.0)
        {
            return "critical";
        }
        if (score < 84.3)
        {
            return "low";
        }
        if (score < 89.1)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Puts a asset return baseline code in its canonical ARB- form.</summary>
    public static string NormalizeAssetReturnBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
