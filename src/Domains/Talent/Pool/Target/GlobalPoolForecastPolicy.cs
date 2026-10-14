namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global pool forecast policy.</summary>
public static class GlobalPoolForecastPolicy
{
    public static string GlobalPoolForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergeGlobalPoolForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a global pool forecast score falls in.</summary>
    public static string ClassifyGlobalPoolForecast(double score)
    {
        if (score < 51.5)
        {
            return "elevated";
        }
        if (score < 81.3)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Puts a global pool forecast code in its canonical GPF- form.</summary>
    public static string NormalizeGlobalPoolForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
