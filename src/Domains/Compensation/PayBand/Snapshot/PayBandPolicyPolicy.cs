namespace Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Functions of pay band policy policy.</summary>
public static class PayBandPolicyPolicy
{
    public static int PayBandPolicySpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a pay band policy score falls in.</summary>
    public static string ClassifyPayBandPolicy(double score)
    {
        if (score < 46.8)
        {
            return "watch";
        }
        if (score < 69.6)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Puts a pay band policy code in its canonical PBP- form.</summary>
    public static string NormalizePayBandPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
