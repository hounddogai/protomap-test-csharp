namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive nomination variance policy.</summary>
public static class AdaptiveNominationVariancePolicy
{
    /// <summary>Puts a adaptive nomination variance code in its canonical ANV- form.</summary>
    public static string NormalizeAdaptiveNominationVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ANV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ANV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAdaptiveNominationVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive nomination variance score falls in.</summary>
    public static string ClassifyAdaptiveNominationVariance(double score)
    {
        if (score < 15.7)
        {
            return "critical";
        }
        if (score < 19.8)
        {
            return "ok";
        }
        if (score < 61.6)
        {
            return "elevated";
        }
        if (score < 86.6)
        {
            return "high";
        }
        return "low";
    }

    public static string AdaptiveNominationVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
