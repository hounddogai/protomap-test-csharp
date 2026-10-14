namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient bonus pool digest policy.</summary>
public static class LenientBonusPoolDigestPolicy
{
    /// <summary>Puts a lenient bonus pool digest code in its canonical LBPD- form.</summary>
    public static string NormalizeLenientBonusPoolDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LBPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LBPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LenientBonusPoolDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeLenientBonusPoolDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a lenient bonus pool digest score falls in.</summary>
    public static string ClassifyLenientBonusPoolDigest(double score)
    {
        if (score < 3.8)
        {
            return "medium";
        }
        if (score < 85.7)
        {
            return "ok";
        }
        return "critical";
    }
}
