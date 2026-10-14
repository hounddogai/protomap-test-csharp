namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant claim policy.</summary>
public static class EquityGrantClaimPolicy
{
    public static double EquityGrantClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a equity grant claim score falls in.</summary>
    public static string ClassifyEquityGrantClaim(double score)
    {
        if (score < 13.0)
        {
            return "watch";
        }
        if (score < 32.2)
        {
            return "low";
        }
        if (score < 76.3)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Puts a equity grant claim code in its canonical EGC- form.</summary>
    public static string NormalizeEquityGrantClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int EquityGrantClaimSpan(int start, int end)
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
}
