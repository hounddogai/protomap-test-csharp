namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation preference policy.</summary>
public static class AttestationPreferencePolicy
{
    /// <summary>Names the band a attestation preference score falls in.</summary>
    public static string ClassifyAttestationPreference(double score)
    {
        if (score < 11.7)
        {
            return "elevated";
        }
        if (score < 48.4)
        {
            return "low";
        }
        if (score < 80.0)
        {
            return "medium";
        }
        return "watch";
    }

    public static double AttestationPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a attestation preference code in its canonical AP- form.</summary>
    public static string NormalizeAttestationPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
