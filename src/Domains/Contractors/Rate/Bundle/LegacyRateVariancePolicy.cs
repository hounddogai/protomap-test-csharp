namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy rate variance policy.</summary>
public static class LegacyRateVariancePolicy
{
    public static double LegacyRateVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy rate variance score falls in.</summary>
    public static string ClassifyLegacyRateVariance(double score)
    {
        if (score < 45.6)
        {
            return "critical";
        }
        if (score < 53.5)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Puts a legacy rate variance code in its canonical LRV- form.</summary>
    public static string NormalizeLegacyRateVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
