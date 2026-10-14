namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending attestation threshold policy.</summary>
public static class PendingAttestationThresholdPolicy
{
    /// <summary>Puts a pending attestation threshold code in its canonical PAT- form.</summary>
    public static string NormalizePendingAttestationThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePendingAttestationThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
