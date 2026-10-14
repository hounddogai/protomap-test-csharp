namespace Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Functions of fallback position quota policy.</summary>
public static class FallbackPositionQuotaPolicy
{
    /// <summary>Puts a fallback position quota code in its canonical FPQ- form.</summary>
    public static string NormalizeFallbackPositionQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a fallback position quota score falls in.</summary>
    public static string ClassifyFallbackPositionQuota(double score)
    {
        if (score < 26.7)
        {
            return "medium";
        }
        if (score < 43.4)
        {
            return "low";
        }
        return "ok";
    }

    public static int FallbackPositionQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 10665.0, in order.</summary>
    public static int FallbackPositionQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10665.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
