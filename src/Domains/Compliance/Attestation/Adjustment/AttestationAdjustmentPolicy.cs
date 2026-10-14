namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation adjustment policy.</summary>
public static class AttestationAdjustmentPolicy
{
    public static double AttestationAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a attestation adjustment code in its canonical AA- form.</summary>
    public static string NormalizeAttestationAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
