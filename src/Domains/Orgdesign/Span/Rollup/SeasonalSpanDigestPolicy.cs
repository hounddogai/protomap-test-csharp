namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal span digest policy.</summary>
public static class SeasonalSpanDigestPolicy
{
    /// <summary>Names the band a seasonal span digest score falls in.</summary>
    public static string ClassifySeasonalSpanDigest(double score)
    {
        if (score < 3.9)
        {
            return "critical";
        }
        if (score < 17.3)
        {
            return "minimal";
        }
        if (score < 55.2)
        {
            return "ok";
        }
        if (score < 81.8)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Puts a seasonal span digest code in its canonical SSD- form.</summary>
    public static string NormalizeSeasonalSpanDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeSeasonalSpanDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
