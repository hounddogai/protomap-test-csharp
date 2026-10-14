namespace Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Functions of weighted attestation entry policy.</summary>
public static class WeightedAttestationEntryPolicy
{
    /// <summary>Puts a weighted attestation entry code in its canonical WAE- form.</summary>
    public static string NormalizeWeightedAttestationEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WAE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WAE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted attestation entry score falls in.</summary>
    public static string ClassifyWeightedAttestationEntry(double score)
    {
        if (score < 21.3)
        {
            return "medium";
        }
        if (score < 89.0)
        {
            return "ok";
        }
        return "watch";
    }
}
