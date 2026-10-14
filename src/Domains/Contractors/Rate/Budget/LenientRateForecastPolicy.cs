namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient rate forecast policy.</summary>
public static class LenientRateForecastPolicy
{
    public static int LenientRateForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a lenient rate forecast code in its canonical LRF- form.</summary>
    public static string NormalizeLenientRateForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLenientRateForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a lenient rate forecast score falls in.</summary>
    public static string ClassifyLenientRateForecast(double score)
    {
        if (score < 46.9)
        {
            return "critical";
        }
        if (score < 85.0)
        {
            return "elevated";
        }
        return "minimal";
    }
}
