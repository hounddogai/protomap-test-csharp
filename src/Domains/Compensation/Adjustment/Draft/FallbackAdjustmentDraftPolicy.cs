namespace Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>Functions of fallback adjustment draft policy.</summary>
public static class FallbackAdjustmentDraftPolicy
{
    /// <summary>Names the band a fallback adjustment draft score falls in.</summary>
    public static string ClassifyFallbackAdjustmentDraft(double score)
    {
        if (score < 6.4)
        {
            return "watch";
        }
        if (score < 15.5)
        {
            return "high";
        }
        if (score < 16.2)
        {
            return "ok";
        }
        if (score < 87.7)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Puts a fallback adjustment draft code in its canonical FAD- form.</summary>
    public static string NormalizeFallbackAdjustmentDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FAD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FAD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
