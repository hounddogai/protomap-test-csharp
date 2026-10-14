namespace Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Functions of pay band threshold policy.</summary>
public static class PayBandThresholdPolicy
{
    /// <summary>Puts a pay band threshold code in its canonical PBT- form.</summary>
    public static string NormalizePayBandThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pay band threshold score falls in.</summary>
    public static string ClassifyPayBandThreshold(double score)
    {
        if (score < 35.6)
        {
            return "watch";
        }
        if (score < 80.8)
        {
            return "high";
        }
        return "medium";
    }
}
