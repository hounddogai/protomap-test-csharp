namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation profile policy.</summary>
public static class AttestationProfilePolicy
{
    public static List<string> MergeAttestationProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a attestation profile code in its canonical AP- form.</summary>
    public static string NormalizeAttestationProfileCode(string code)
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
