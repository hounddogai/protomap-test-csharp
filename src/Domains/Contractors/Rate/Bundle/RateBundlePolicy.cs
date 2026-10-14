namespace Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Functions of rate bundle policy.</summary>
public static class RateBundlePolicy
{
    /// <summary>Names the band a rate bundle score falls in.</summary>
    public static string ClassifyRateBundle(double score)
    {
        if (score < 80.8)
        {
            return "high";
        }
        if (score < 84.0)
        {
            return "minimal";
        }
        if (score < 93.7)
        {
            return "low";
        }
        if (score < 99.5)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Puts a rate bundle code in its canonical RB- form.</summary>
    public static string NormalizeRateBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RateBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
