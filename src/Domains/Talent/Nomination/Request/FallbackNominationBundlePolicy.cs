namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback nomination bundle policy.</summary>
public static class FallbackNominationBundlePolicy
{
    public static List<string> MergeFallbackNominationBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a fallback nomination bundle code in its canonical FNB- form.</summary>
    public static string NormalizeFallbackNominationBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FNB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FNB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackNominationBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a fallback nomination bundle score falls in.</summary>
    public static string ClassifyFallbackNominationBundle(double score)
    {
        if (score < 39.6)
        {
            return "high";
        }
        if (score < 45.1)
        {
            return "ok";
        }
        if (score < 82.8)
        {
            return "minimal";
        }
        if (score < 89.0)
        {
            return "watch";
        }
        return "low";
    }
}
