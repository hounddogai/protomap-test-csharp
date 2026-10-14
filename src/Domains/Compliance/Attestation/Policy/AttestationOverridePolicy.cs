namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation override policy.</summary>
public static class AttestationOverridePolicy
{
    /// <summary>Puts a attestation override code in its canonical AO- form.</summary>
    public static string NormalizeAttestationOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AttestationOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
