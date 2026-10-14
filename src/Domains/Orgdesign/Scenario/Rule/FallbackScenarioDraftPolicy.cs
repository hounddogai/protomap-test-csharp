namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback scenario draft policy.</summary>
public static class FallbackScenarioDraftPolicy
{
    public static List<string> MergeFallbackScenarioDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a fallback scenario draft code in its canonical FSD- form.</summary>
    public static string NormalizeFallbackScenarioDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a fallback scenario draft score falls in.</summary>
    public static string ClassifyFallbackScenarioDraft(double score)
    {
        if (score < 14.2)
        {
            return "ok";
        }
        if (score < 60.4)
        {
            return "minimal";
        }
        if (score < 64.9)
        {
            return "elevated";
        }
        if (score < 93.6)
        {
            return "medium";
        }
        return "low";
    }

    public static string FallbackScenarioDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
