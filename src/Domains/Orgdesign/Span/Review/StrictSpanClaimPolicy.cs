namespace Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Functions of strict span claim policy.</summary>
public static class StrictSpanClaimPolicy
{
    /// <summary>Puts a strict span claim code in its canonical SSC- form.</summary>
    public static string NormalizeStrictSpanClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a strict span claim score falls in.</summary>
    public static string ClassifyStrictSpanClaim(double score)
    {
        if (score < 5.3)
        {
            return "medium";
        }
        if (score < 41.0)
        {
            return "elevated";
        }
        if (score < 51.5)
        {
            return "low";
        }
        if (score < 77.3)
        {
            return "watch";
        }
        return "minimal";
    }
}
