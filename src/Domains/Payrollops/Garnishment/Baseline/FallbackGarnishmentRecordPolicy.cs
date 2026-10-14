namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Functions of fallback garnishment record policy.</summary>
public static class FallbackGarnishmentRecordPolicy
{
    public static int FallbackGarnishmentRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a fallback garnishment record score falls in.</summary>
    public static string ClassifyFallbackGarnishmentRecord(double score)
    {
        if (score < 47.5)
        {
            return "ok";
        }
        if (score < 54.8)
        {
            return "low";
        }
        if (score < 72.7)
        {
            return "minimal";
        }
        if (score < 79.8)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Puts a fallback garnishment record code in its canonical FGR- form.</summary>
    public static string NormalizeFallbackGarnishmentRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FGR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FGR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
