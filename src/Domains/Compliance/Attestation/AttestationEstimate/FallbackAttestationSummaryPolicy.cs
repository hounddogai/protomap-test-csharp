namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback attestation summary policy.</summary>
public static class FallbackAttestationSummaryPolicy
{
    public static double FallbackAttestationSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback attestation summary code in its canonical FAS- form.</summary>
    public static string NormalizeFallbackAttestationSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a fallback attestation summary score falls in.</summary>
    public static string ClassifyFallbackAttestationSummary(double score)
    {
        if (score < 31.5)
        {
            return "ok";
        }
        if (score < 49.7)
        {
            return "critical";
        }
        if (score < 63.9)
        {
            return "elevated";
        }
        if (score < 72.2)
        {
            return "medium";
        }
        return "minimal";
    }
}
