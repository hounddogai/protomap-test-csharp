namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation entry policy.</summary>
public static class AttestationEntryPolicy
{
    /// <summary>Puts a attestation entry code in its canonical AE- form.</summary>
    public static string NormalizeAttestationEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AttestationEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
